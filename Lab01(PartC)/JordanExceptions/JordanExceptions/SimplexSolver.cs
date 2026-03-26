using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class SimplexSolver
    {
        private string[] _rows;
        private string[] _columns;


        public int _rowsEqualityCount { get; set; }

        private string[] Rows 
        {
            get { return _rows; }
            set { _rows = value;}
        }
        private string[] Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }


        private readonly ISaveProtocol _protocol;
        private readonly IJordanMethod _jordan;
        private readonly MinimalPositiveRatioFinder _minRatio;
        private readonly ConstraintTransformer _transformer;

        public int RowsEqualityCount { get; set; }
        public SimplexSolver(ISaveProtocol protocol, IJordanMethod jordan) 
        { 
            _protocol = protocol;            
            _jordan = jordan;
            _minRatio = new MinimalPositiveRatioFinder();
            _transformer = new ConstraintTransformer(_protocol, _jordan, this);
        }



        public double[,] FindInitialFeasibleSolution(double[,] MatrixA, int totalX)
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
                        
                _protocol.SaveStepHeader(Rows[row], Columns[column], "Пошук опорного розв'язку");

                      
                (Columns[column], Rows[row]) = (Rows[row], Columns[column]);
                      
                MatrixA = _jordan.MatrixSolver(MatrixA, pivot, row, column);
                       
                _protocol.SaveTable(MatrixA, Rows, Columns);                       
                _protocol.SaveSectionHeader("ОПОРНИЙ РОЗВ'ЯЗОК: ");                      
                double[] resX = GenerateResult(MatrixA, totalX);                       
                _protocol.ResultSimplexSave(resX);

                       
            }


        }
    
        
          
        




        public double[] FindOptimalSolution(double[,] MatrixA)
        {

            int rowsCount = MatrixA.GetLength(0) - 1;
            int colsCount = MatrixA.GetLength(1) - 1;

            InitializeLabels(rowsCount, colsCount);
            int totalX = MatrixA.GetLength(1);

            _protocol.InputTableSave(MatrixA, Rows, Columns);

            if (Rows.Contains("0"))
            {
                MatrixA = _transformer.ToCanonicalForm(MatrixA, ref _columns, ref _rows);
            }

            rowsCount = MatrixA.GetLength(0) - 1;
            colsCount = MatrixA.GetLength(1) - 1;
            MatrixA = FindInitialFeasibleSolution(MatrixA, totalX);


            bool optimal = false;

            while (!optimal)
            {
                optimal = true;
                for (int j = 0; j < colsCount; j++)
                {
                    if (MatrixA[rowsCount, j] < 0)
                    {
                        int row = _minRatio.FindMinPositiveRatio(MatrixA, j);
                        if (row < 0) throw new Exception("Функція не має максимуму/мінімуму");

                        _protocol.SaveStepHeader(Rows[row], Columns[j], "Пошук оптимального розв'язку");

                        MatrixA = _jordan.MatrixSolver(MatrixA, MatrixA[row, j], row, j);
                        (Columns[j], Rows[row]) = (Rows[row], Columns[j]);

                        _protocol.SaveTable(MatrixA, Rows, Columns);

                        optimal = false;
                        break;
                    }
                }
            }

            double[] resX = GenerateResult(MatrixA, totalX);

            resX[resX.Length-1] = MatrixA[rowsCount, colsCount];

            return resX;
        }

        public void InitializeLabels(int rows, int cols)
        {
            int n = 0;
            Rows = new string[rows+1];

            for (int i = 0; i < rows-RowsEqualityCount; i++)
            {
                Rows[i] = "y" + (i + 1);
                n++;
            }

            for (int i = n; i < rows; i++)
            {
                Rows[i] = "0";
            }

            Rows[rows] = "Z";
            
            Columns = new string[cols+1];
            for (int j = 0; j < cols; j++)
            {
                Columns[j] = "x" + (j + 1);
            }
            Columns[cols] = "1";           
        }


        private double[] GenerateResult(double[,] MatrixA, int totalX)
        {
            int rowsCount = MatrixA.GetLength(0) - 1;
            int colsCount = MatrixA.GetLength(1) - 1;

            double[] x = new double[totalX];


            for (int i = 0; i < rowsCount; i++)
            {
                string name = Rows[i];

                if (name.StartsWith("x"))
                {
                    string numberPart = name.Substring(1);

                    if (int.TryParse(numberPart, out int index))
                    {
                        int arrayIndex = index - 1;

                        if (arrayIndex >= 0 && arrayIndex < x.Length)
                        {
                            x[arrayIndex] = MatrixA[i, colsCount];
                        }
                    }
                }
            }

            return x;
        }
    }
}
