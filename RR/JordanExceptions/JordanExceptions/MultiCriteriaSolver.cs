using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JordanExceptions
{
    public sealed class MultiCriteriaSolver
    {
        private readonly SimplexSolver _simplex;
        private readonly DualSimplexSolver _dual;
        private readonly MatrixAnalyzer _analyzer;
        private readonly ISaveProtocol _protocol;

        public MultiCriteriaSolver(SimplexSolver simplex, DualSimplexSolver dual, MatrixAnalyzer analyzer, ISaveProtocol protocol)
        {
            _simplex = simplex;
            _dual = dual;
            _analyzer = analyzer;
            _protocol = protocol;
        }

        public MultiCriteriaResult Solve(string objectivesBlock, string equalitiesBlock)
        {
            var objectiveLines = objectivesBlock
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .ToList();

            string[] eqLines = equalitiesBlock
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .ToArray();

            int n = GetMaxVarIndex(equalitiesBlock + " " + objectivesBlock);
            int k = objectiveLines.Count;

            var isMax = new bool[k];
            var coeffs = new double[k][];
            for (int p = 0; p < k; p++)
            {
                string compactObj = Regex.Replace(objectiveLines[p], @"\s+", "");
                var dir = Regex.Match(compactObj, @"(max|min)$", RegexOptions.IgnoreCase);
                isMax[p] = string.Equals(dir.Groups[1].Value, "max", StringComparison.OrdinalIgnoreCase);
                string expr = compactObj.Substring(0, compactObj.Length - dir.Value.Length);
                coeffs[p] = ParseObjectiveCoeffsFromLhs(expr, n);
            }

            double[,] template = BuildEqualitiesTable(eqLines, n);
            var optimalX = new double[k][];
            var optVal = new double[k];


            //Початок розв`язку

            for (int p = 0; p < k; p++)
            {
                double[,] mat = (double[,])template.Clone();
                FillObjectiveRow(mat, coeffs[p], n);
                if (isMax[p]) PrepareForMax(mat);

                _simplex.Reset();
                _simplex.RowsEqualityCount = eqLines.Length;
                double[] sol = _simplex.FindOptimalSolution(mat);
                optimalX[p] = ExtractDecisionVector(sol, n);
                optVal[p] = Dot(coeffs[p], optimalX[p], n);
            }

            var matrixQ = new double[k, k];
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    double currentVal = Dot(coeffs[j], optimalX[i], n);
                    double optimalVal = optVal[j];

                    if (Math.Abs(optimalVal) < 1e-12)
                        matrixQ[i, j] = 0;
                    else
                        matrixQ[i, j] = Math.Abs((currentVal - optimalVal) / optimalVal);
                }
            }

            var payoffA = new double[k, k];
            for (int i = 0; i < k; i++)
                for (int j = 0; j < k; j++)
                    payoffA[i, j] = -matrixQ[i, j];
        
            _protocol.SaveSectionHeader("Розв'язання матричної гри");
            _protocol.StepSave("\nМатриця гри: \n");
            _protocol.SaveMatrix(payoffA);

            string lpForm = BuildMatrixGameLpFormulation(payoffA, k);
            CalculationResult game = SolveMatrixGame(payoffA);
            double[] weights = NormalizeWeights(game.P, k);
           
            _protocol.SaveSectionHeader("Результати ігрової задачі");
            _protocol.StepSave($"Ціна гри V = {game.V.ToString("F2", CultureInfo.InvariantCulture)}\n");
            _protocol.StepSave("Стратегії першого гравця:");

            for (int i = 0; i < k; i++)
            {
                _protocol.StepSave($"  x{i + 1} = {weights[i].ToString("F1", CultureInfo.InvariantCulture)}");
            }

            var compromise = new double[n];
            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < k; i++)
                    compromise[j] += weights[i] * optimalX[i][j];
            }

            _protocol.SaveSectionHeader("ФІНАЛЬНИЙ КОМПРОМІСНИЙ РОЗВ'ЯЗОК");
            string xStar = string.Join("; ", compromise.Select(v => v.ToString("F2", CultureInfo.InvariantCulture)));
            _protocol.StepSave($"X* = ({xStar})\n");

            return new MultiCriteriaResult
            {
                ObjectiveCoefficients = ConvertToMatrix(coeffs, k, n),
                IsMax = isMax,
                OptimalPlans = ConvertToMatrix(optimalX, k, n),
                RegretMatrix = matrixQ,
                MatrixGameLpFormulation = lpForm,
                Weights = weights,
                CompromisePlan = compromise,
                OptimalCriterionValues = optVal,
                GameValue = game.V
            };
        }

        private CalculationResult SolveMatrixGame(double[,] a)
        {
            int rows = a.GetLength(0);
            int cols = a.GetLength(1);

            _analyzer.InactiveRows.Clear();
            _analyzer.InactiveCols.Clear();
            _simplex.Reset();
            _dual.Reset();
            _simplex.RowsEqualityCount = 0;
            _dual.RowsEqualityCount = 0;

            double[] resX = new double[rows];
            double[] resU = new double[cols];
            double vFinal = 0;

            double[] pureRes = _analyzer.PureStrategy(a);
            if (pureRes != null)
            {
                int bestRow = (int)pureRes[0];
                int bestCol = (int)pureRes[1];
                vFinal = pureRes[2];
                for (int i = 0; i < rows; i++) resX[i] = (i == bestRow) ? 1.0 : 0.0;
                for (int j = 0; j < cols; j++) resU[j] = (j == bestCol) ? 1.0 : 0.0;
                return new CalculationResult { P = resX, Q = resU, V = vFinal };
            }

            var workingMatrix = (double[,])a.Clone();
            double minVal = workingMatrix.Cast<double>().Min();
            double kShift = (minVal < -1e-9) ? Math.Abs(minVal): 0;

            if (kShift > 0)
            {
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++)
                        workingMatrix[i, j] += kShift;
            }

            double[,] extended = PrepareExtendedMatrix(workingMatrix);

            _protocol.SaveSectionHeader("Вхідна симплекс-таблиця матричної гри");
            _protocol.SaveMatrix(extended);

            double[] rawX = _simplex.FindOptimalSolution(extended);
            double[] rawU = _dual.FindOptimalSolution(extended);
            double Z = rawX[rawX.Length - 1];

            vFinal = (1 / Z);

            int rIdx = 0;
            for (int i = 0; i < rows; i++)
            {
                if (!_analyzer.InactiveRows.Contains(i)) resX[i] = rawU[rIdx++] / Z;
                else resX[i] = 0;
            }

            int cIdx = 0;
            for (int j = 0; j < cols; j++)
            {
                if (!_analyzer.InactiveCols.Contains(j)) resU[j] = rawX[cIdx++] / Z;
                else resU[j] = 0;
            }

            return new CalculationResult { P = resX, Q = resU, V = vFinal };
        }

        private static string BuildMatrixGameLpFormulation(double[,] a, int k)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Пряма задача (max): Z = q1 + ... + qk");
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                    sb.Append($"({a[i, j]:F2})*q{j + 1} ");
                sb.AppendLine("<= 1");
            }
            return sb.ToString();
        }



        // --- Допоміжні методи перетворення та обчислення ---

        private static double[,] ConvertToMatrix(double[][] source, int r, int c)
        {
            var m = new double[r, c];
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    m[i, j] = source[i][j];
            return m;
        }

        private static double Dot(double[] c, double[] x, int n) => Enumerable.Range(0, n).Sum(i => c[i] * x[i]);

        private static int GetMaxVarIndex(string text)
        {
            var matches = Regex.Matches(text, @"x(\d+)");
            return matches.Count > 0 ? matches.Cast<Match>().Max(m => int.Parse(m.Groups[1].Value)) : 0;
        }

        private static double[] ParseObjectiveCoeffsFromLhs(string lhs, int n)
        {
            var c = new double[n];
            foreach (Match m in Regex.Matches(lhs, @"([+-]?(?:\d+[.,]\d+|\d+)?)x(\d+)"))
            {
                int idx = int.Parse(m.Groups[2].Value) - 1;
                if (idx >= n) continue;
                string co = m.Groups[1].Value;
                c[idx] = (co == "" || co == "+") ? 1 : (co == "-") ? -1 : double.Parse(co.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            return c;
        }
        private double[,] PrepareExtendedMatrix(double[,] shiftedMatrix)
        {
            int rows = shiftedMatrix.GetLength(0);
            int cols = shiftedMatrix.GetLength(1);

            double[,] extendedMatrix = new double[rows + 1, cols + 1];

            for (int i = 0; i < rows + 1; i++)
            {
                for (int j = 0; j < cols + 1; j++)
                {
                    if (i < rows && j < cols)
                    {
                        extendedMatrix[i, j] = shiftedMatrix[i, j];
                    }
                    else if (i < rows && j == cols)
                    {
                        extendedMatrix[i, j] = 1;
                    }
                    else if (i == rows && j < cols)
                    {
                        extendedMatrix[i, j] = -1;
                    }
                    else
                    {
                        extendedMatrix[i, j] = 0;
                    }
                }
            }

            return extendedMatrix;
        }
        private static double[,] BuildEqualitiesTable(string[] lines, int n)
        {
            var res = new double[lines.Length + 1, n + 1];
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split('=');
                foreach (Match m in Regex.Matches(parts[0], @"([+-]?(?:\d+[.,]\d+|\d+)?)x(\d+)"))
                {
                    int idx = int.Parse(m.Groups[2].Value) - 1;
                    if (idx >= n) continue;
                    string co = m.Groups[1].Value;
                    res[i, idx] = (co == "" || co == "+") ? 1 : (co == "-") ? -1 : double.Parse(co.Replace(',', '.'), CultureInfo.InvariantCulture);
                }
                res[i, n] = double.Parse(parts[1].Trim().Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            return res;
        }

        private static void FillObjectiveRow(double[,] m, double[] c, int n) { for (int j = 0; j < n; j++) m[m.GetLength(0) - 1, j] = c[j]; }
        private static void PrepareForMax(double[,] m) { int r = m.GetLength(0) - 1; for (int j = 0; j < m.GetLength(1) - 1; j++) m[r, j] *= -1; }
        private static double[] ExtractDecisionVector(double[] s, int n) => s.Take(n).ToArray();
        private static double[] NormalizeWeights(double[] p, int k)
        {
            double sum = p.Sum();
            return sum < 1e-12 ? Enumerable.Repeat(1.0 / k, k).ToArray() : p.Select(v => v / sum).ToArray();
        }
    }
}

public class MultiCriteriaResult
    {
        public required double[,] ObjectiveCoefficients { get; init; }
        public required bool[] IsMax { get; init; }
        public required double[,] OptimalPlans { get; init; }
        public required double[,] RegretMatrix { get; init; }
        public string MatrixGameLpFormulation { get; init; } = "";
        public required double[] Weights { get; init; }
        public required double[] CompromisePlan { get; init; }
        public required double[] OptimalCriterionValues { get; init; }
        public double GameValue { get; init; }
    }
