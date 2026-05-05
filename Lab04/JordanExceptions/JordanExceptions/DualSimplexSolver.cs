using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class DualSimplexSolver: SimplexSolver
    {
        private string[] DualRows;
        private string[] DualColumns;

        public DualSimplexSolver(ISaveProtocol protocol, IJordanMethod jordan) : base(protocol, jordan) { }


        public override void InitializeLabels(int rows, int cols)
        {
            base.InitializeLabels(rows, cols);

            DualRows = new string[rows + 1];
            DualColumns = new string[cols + 1];

            for (int i = 0; i < rows; i++)
            {
                DualRows[i] = "u" + (i + 1);
            }
            DualRows[rows] = "1";

            for (int j = 0; j < cols; j++)
            {
                DualColumns[j] = "v" + (j + 1);
            }
            DualColumns[cols] = "W";
        }

        private (string[] r, string[] c) GetCombinedLabels()
        {
            if (_rows == null || DualRows == null || _columns == null || DualColumns == null)
            {
                return (new string[0], new string[0]);
            }

            var r = _rows.Zip(DualRows, (p, d) => $"{p}({d})").ToArray();
            var c = _columns.Zip(DualColumns, (p, d) => $"{p}({d})").ToArray();
            
            return (r, c);
        }

        protected override void SwapLabels(int row, int col)
        {
            base.SwapLabels(row, col);
            (DualColumns[col], DualRows[row]) = (DualRows[row], DualColumns[col]);
        }

        public double[] GenerateDualResult(double[,] MatrixA, int totalDualVariables)
        {
            int rowsCount = MatrixA.GetLength(0) - 1;
            int colsCount = MatrixA.GetLength(1) - 1;

            double[] dualResult = new double[totalDualVariables];

            for (int j = 0; j < colsCount; j++)
            {
                string dName = DualColumns[j];
                if (dName != null && dName.Contains("u"))
                {
                    string numericPart = new string(dName.Where(char.IsDigit).ToArray());

                    if (int.TryParse(numericPart, out int index))
                    {
                        int arrayIndex = index - 1;
                        if (arrayIndex >= 0 && arrayIndex < dualResult.Length)
                        {
                            dualResult[arrayIndex] = MatrixA[rowsCount, j];
                        }
                    }
                }
            }

            return dualResult;
        }

        public override double[,] FindInitialFeasibleSolution(double[,] MatrixA, int totalX)
        {
            int rowsCount = MatrixA.GetLength(0) - 1;
            int colsCount = MatrixA.GetLength(1) - 1;

            while (true)
            {
                int row = -1;

                for (int i = 0; i < rowsCount; i++)
                {
                    if (MatrixA[i, colsCount] < 0)
                    {
                        row = i;
                        break;
                    }
                }

                if (row == -1)
                {
                    _protocol.SaveSectionHeader("ЗНАЙДЕНО ОПОРНИЙ РОЗВ'ЯЗОК");

                    double[] res = GenerateResult(MatrixA, totalX);
                    _protocol.ResultSimplexSave(res);

                    return MatrixA;
                }

                int column = -1;
                for (int j = 0; j < colsCount; j++)
                {
                    if (_columns[j].Contains("0"))
                        continue;

                    if (MatrixA[row, j] < 0)
                    {
                        column = j;
                        break;
                    }
                }

                if (column == -1)
                {
                    throw new Exception("Система обмежень суперечлива (немає опорного розв'язку)");
                }

                row = _minRatio.FindMinPositiveRatio(MatrixA, column);
                double pivot = MatrixA[row, column];

                _protocol.SaveStepHeader(
                    $"{_rows[row]}\n{DualRows[row]}",
                    $"{_columns[column]}\n{DualColumns[column]}",
                    "Пошук опорного розв'язку (Крок Jordan)"
                );

                SwapLabels(row, column);

                MatrixA = _jordan.MatrixSolver(MatrixA, pivot, row, column);

                var currentLabels = GetCombinedLabels();
                _protocol.SaveTable(MatrixA, currentLabels.r, currentLabels.c);

                _protocol.SaveSectionHeader("ПРОМІЖНИЙ СТАН: ");
                double[] resX = GenerateResult(MatrixA, totalX);
                _protocol.ResultSimplexSave(resX);
            }
        }


        public override double[] FindOptimalSolution(double[,] MatrixA)
        {
            int rowsCount = MatrixA.GetLength(0) - 1;
            int colsCount = MatrixA.GetLength(1) - 1;

            InitializeLabels(rowsCount, colsCount);
            int totalX = MatrixA.GetLength(1);

            var initialLabels = GetCombinedLabels();
            _protocol.InputTableSave(MatrixA, initialLabels.r, initialLabels.c);

            if (_rows.Contains("0"))
            {
                MatrixA = _transformer.ToCanonicalForm(MatrixA, ref _columns, ref _rows, ref DualColumns, ref DualRows);

                rowsCount = MatrixA.GetLength(0) - 1;
                colsCount = MatrixA.GetLength(1) - 1;
            }

            MatrixA = FindInitialFeasibleSolution(MatrixA, totalX);

            bool optimal = false;
            while (!optimal)
            {
                optimal = true;
                rowsCount = MatrixA.GetLength(0) - 1;
                colsCount = MatrixA.GetLength(1) - 1;

                for (int j = 0; j < colsCount; j++)
                {
                    if (_columns[j] == "0")
                    {
                        continue;
                    }
                    if (MatrixA[rowsCount, j] < 0)
                    {
                        int row = _minRatio.FindMinPositiveRatio(MatrixA, j);
                        if (row < 0) throw new Exception("Функція не обмежена (немає максимуму/мінімуму)");

                        _protocol.SaveStepHeader(
                            $"{_rows[row]}\n{DualRows[row]}",
                            $"{_columns[j]}\n{DualColumns[j]}",
                            "Пошук оптимального розв'язку (Крок Jordan)"
                        );

                        double pivot = MatrixA[row, j];
                        MatrixA = _jordan.MatrixSolver(MatrixA, pivot, row, j);

                        SwapLabels(row, j);

                        var currentLabels = GetCombinedLabels();
                        _protocol.SaveTable(MatrixA, currentLabels.r, currentLabels.c);

                        optimal = false;
                        break;
                    }
                }
            }

            double[] resU = GenerateDualResult(MatrixA, rowsCount);
        
            double[] finalResult = new double[resU.Length + 1];
            Array.Copy(resU, finalResult, resU.Length);

            finalResult[finalResult.Length - 1] = MatrixA[rowsCount, colsCount];

            this.LastMatrix = MatrixA;

            return finalResult;
        }
    }
}
