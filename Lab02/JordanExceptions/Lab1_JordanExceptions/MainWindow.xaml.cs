using JordanExceptions;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab1_JordanExceptions
{
    public partial class MainWindow : Window
    {
        private readonly RankSolver _rankSolver;
        private readonly SimplexSolver _solver;
        private readonly LinearSolver _linearSolver;
        private readonly IMatrixInvertor _invertor;
        private readonly ISaveProtocol _protocol;
        private readonly GomorySimplex _intsolver;
        private string fullPath { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _protocol = new SaveProtocol();
            var jordan = new JordanMethod();
            _rankSolver = new RankSolver(_protocol, jordan);
            _invertor = new MatrixInvertor(jordan, _protocol);
            _linearSolver = new LinearSolver(_protocol, _invertor);
            _solver = new SimplexSolver(_protocol, jordan);
            _intsolver = new GomorySimplex(_protocol, jordan);
            fullPath = System.IO.Path.GetFullPath("Protocol.txt");

        }



        private void Btn_rank_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _protocol.FileCleaner();

                var matrixA = ParseMatrix(txtbx_A.Text);
                int rank = _rankSolver.GetSolution(matrixA, matrixA.GetLength(0), matrixA.GetLength(1));
                txtbx_rank.Text = rank.ToString();

                txtblk_protocol.Text = $"Протокол обичслень створено за посиланням: {fullPath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при розрахунку рангу: " + ex.Message);
            }
        }



        private void Btn_result_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _protocol.FileCleaner();

                var matrixA = ParseMatrix(txtbx_A.Text);
                var vectorB = ParseVector(txtbx_b.Text);

                if (matrixA.GetLength(0) != vectorB.Length)
                {
                    throw new Exception("Кількість рядків A має дорівнювати довжині b!");
                }

                double[] xResult = _linearSolver.GetSolution(matrixA, vectorB);

                txtbx_X.Text = string.Join(Environment.NewLine, xResult.Select(v => v.ToString("F2")));

                txtblk_protocol.Text = $"Протокол обичслень створено за посиланням: {fullPath}";

            }
            catch (Exception)
            {
                MessageBox.Show("Помилка обчислення СЛАР");
            }
        }



        private void Btn_invert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _protocol.FileCleaner();
                var matrixA = ParseMatrix(txtbx_A.Text);
                double[,] inverted = _invertor.InvertMatrix(matrixA);

                txtbx_invert.Text = FormatMatrixToString(inverted);

                txtblk_protocol.Text = $"Протокол обичслень створено за посиланням: {fullPath}";

            }
            catch (Exception)
            {
                MessageBox.Show("Матриця вироджена або сталася помилка");
            }
        }


        private double[,] ParseMatrix(string input)
        {
            var lines = input.Trim().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int rows = lines.Length;
            int cols = lines[0].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

            double[,] matrix = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                var values = lines[i].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = double.Parse(values[j].Replace('.', ','));
                }
            }
            return matrix;
        }

        private double[] ParseVector(string input)
        {
            return input.Trim()
                .Split(new[] { '\n', '\r', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => double.Parse(v.Replace('.', ',')))
                .ToArray();
        }

        private string FormatMatrixToString(double[,] matrix)
        {
            string result = "";
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    result += $"{matrix[i, j],8:F2} ";
                }
                result += Environment.NewLine;
            }
            return result;
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }



        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _protocol.FileCleaner();

                double[,] matrix = ParseToSimplexTable(txtConstraints.Text, txtEquality.Text, txtZFunction.Text);
              
                bool isMax = rbMax.IsChecked ?? false; 

                if (isMax) matrix = PrepareForMax(matrix);

                _solver.Reset();

                double[] resultX = _solver.FindOptimalSolution(matrix);
                double[] resultIntX = _intsolver.Solver(matrix);

                txtResultX.Text = $"({string.Join("; ", resultX.Take(resultX.Length - 1).Select(x => x.ToString("F2")))})";
                txtIntResultX.Text = $"({string.Join("; ", resultIntX.Take(resultIntX.Length - 1).Select(x => x.ToString("F2")))})";


                if (isMax){

                    txtResultZ.Text = resultX[resultX.Length - 1].ToString("F2");
                    _protocol.SaveSectionHeader("ОПТИМАЛЬНИЙ ДРОБОВИЙ РОЗВ'ЯЗОК: ");
                    _protocol.ResultSimplexSave(resultX, resultX[resultX.Length - 1]);


                    txtIntResultZ.Text = resultIntX[resultIntX.Length - 1].ToString("F2");
                    _protocol.SaveSectionHeader("ОПТИМАЛЬНИЙ ЦІЛИЙ РОЗВ'ЯЗОК: ");
                    _protocol.ResultSimplexSave(resultIntX, resultIntX[resultIntX.Length - 1]);

                }
                else
                {
                    txtResultZ.Text = (-resultX[resultX.Length - 1]).ToString("F2");
                    _protocol.SaveSectionHeader("ОПТИМАЛЬНИЙ ЦІЛИЙ РОЗВ'ЯЗОК: ");
                    _protocol.ResultSimplexSave(resultX, (-resultX[resultX.Length - 1]));


                    txtIntResultZ.Text = (-resultIntX[resultIntX.Length - 1]).ToString("F2");
                    _protocol.SaveSectionHeader("ОПТИМАЛЬНИЙ ДРОБОВИЙ РОЗВ'ЯЗОК: ");
                    _protocol.ResultSimplexSave(resultIntX, (-resultIntX[resultIntX.Length - 1]));

                }



                txtblk_protocol1.Text = $"Протокол обичслень створено за посиланням: {fullPath}";
            }
            catch (Exception ex)
            {
                txtResultZ.Clear();
                txtResultX.Clear();
                txtIntResultZ.Clear();
                txtIntResultX.Clear();
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка розрахунку", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private double[,] PrepareForMax(double[,] matrix)
        {
            int lastRow = matrix.GetLength(0) - 1;
            int lastCol = matrix.GetLength(1) - 1;

            for (int j = 0; j < lastCol; j++)
            {
                matrix[lastRow, j] *= -1;
            }
            return matrix;
        }




        private double[,] ParseToSimplexTable(string constraints, string equality, string zFunc)
        {
            string[] linesConstraints = constraints.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            string[] linesEquality = equality.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            var allLines = linesConstraints.Concat(linesEquality).ToArray();


            int rowCountConstraints = linesConstraints.Length;
            int rowCountEquality = linesEquality.Length;

            _solver.RowsEqualityCount = rowCountEquality;

            int rowCount = rowCountConstraints + rowCountEquality;


            string fullText = constraints + " " + equality + " " + zFunc;
            var matches = Regex.Matches(fullText, @"x(\d+)");

            int maxVarIndex = 0;
            foreach (Match m in matches)
            {
                int idx = int.Parse(m.Groups[1].Value);
                if (idx > maxVarIndex) maxVarIndex = idx;
            }

            double[,] matrix = new double[rowCount + 1, maxVarIndex + 1];

            for (int i = 0; i < rowCount; i++)
            {
                ParseVariables(allLines[i], matrix, i, maxVarIndex);

                var constMatch = Regex.Match(allLines[i], @"(?:<=|>=|=)\s*(?<const>[+-]?\d+(?:\.\d+)?)");
                if (constMatch.Success)
                {
                    matrix[i, maxVarIndex] = double.Parse(constMatch.Groups["const"].Value.Replace('.', ','));
                }

                if (allLines[i].Contains(">="))
                {
                    for (int j = 0; j <= maxVarIndex; j++) matrix[i, j] *= -1;
                }
            }

            ParseVariables(zFunc, matrix, rowCount, maxVarIndex);

            return matrix;
        }



        private void ParseVariables(string line, double[,] matrix, int row, int maxVar)
        {
            string pattern = @"(?<coeff>[+-]?\d*(?:\.\d+)?)\s*x(?<index>\d+)";
            var matches = Regex.Matches(line, pattern);

            foreach (Match m in matches)
            {
                int index = int.Parse(m.Groups["index"].Value) - 1;

                if (index >= 0 && index < maxVar)
                {
                    string val = m.Groups["coeff"].Value;
                    double c = 1;
                    if (val == "-") c = -1;
                    else if (!string.IsNullOrEmpty(val) && val != "+")
                        c = double.Parse(val.Replace('.', ','));

                    matrix[row, index] = c;
                }
            }
        }



    }
}

