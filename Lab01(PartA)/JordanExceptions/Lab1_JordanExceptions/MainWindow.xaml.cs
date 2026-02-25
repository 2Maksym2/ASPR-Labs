using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JordanExceptions;

namespace Lab1_JordanExceptions
{
    public partial class MainWindow : Window
    {
        private readonly RankSolver _rankSolver;
        private readonly LinearSolver _linearSolver;
        private readonly IMatrixInvertor _invertor;
        private readonly ISaveProtocol _protocol;
        private string fullPath { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _protocol = new SaveProtocol();
            var jordan = new JordanMethod();
            _rankSolver = new RankSolver(_protocol, jordan);
            _invertor = new MatrixInvertor(jordan, _protocol);
            _linearSolver = new LinearSolver(_protocol, _invertor);

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
            catch (Exception ex)
            {
                MessageBox.Show("Помилка СЛАР: " + ex.Message);
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
            catch (Exception ex)
            {
                MessageBox.Show("Матриця вироджена або сталася помилка: " + ex.Message);
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
    }
}
