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
        private string[] _rows { get; set; }
        private string[] _columns { get; set; }


        private readonly ISaveProtocol _protocol;
        private readonly IJordanMethod _jordan;

        public SimplexSolver(ISaveProtocol protocol, IJordanMethod jordan) 
        { 
            _protocol = protocol;            
            _jordan = jordan;
        }



        public double[,] FindInitialFeasibleSolution(double[,] MatrixA)
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
                   
                    double[] res = GenerateResult(MatrixA);
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


                row = FindMinPositiveRatio(MatrixA, column);
                      
                double pivot = MatrixA[row, column];
                        
                _protocol.SaveStepHeader(_rows[row], _columns[column], "Пошук опорного розв'язку");

                      
                (_columns[column], _rows[row]) = (_rows[row], _columns[column]);
                      
                MatrixA = _jordan.MatrixSolver(MatrixA, pivot, row, column);
                       
                _protocol.SaveTable(MatrixA, _rows, _columns);                       
                _protocol.SaveSectionHeader("ОПОРНИЙ РОЗВ'ЯЗОК: ");                      
                double[] resX = GenerateResult(MatrixA);                       
                _protocol.ResultSimplexSave(resX);

                       
            }


        }
    
        
          
        




        public double[] FindOptimalSolution(double[,] MatrixA)
        {

            int rowsCount = MatrixA.GetLength(0) - 1;
            int colsCount = MatrixA.GetLength(1) - 1;

            InitializeLabels(rowsCount, colsCount);
            _protocol.InputTableSave(MatrixA, _rows, _columns);


            MatrixA = FindInitialFeasibleSolution(MatrixA);


            bool optimal = false;

            while (!optimal)
            {
                optimal = true;
                for (int j = 0; j < colsCount; j++)
                {
                    if (MatrixA[rowsCount, j] < 0)
                    {
                        int row = FindMinPositiveRatio(MatrixA, j);
                        if (row < 0) throw new Exception("Функція не має максимуму");

                        _protocol.SaveStepHeader(_rows[row], _columns[j], "Пошук оптимального розв'язку");

                        MatrixA = _jordan.MatrixSolver(MatrixA, MatrixA[row, j], row, j);
                        (_columns[j], _rows[row]) = (_rows[row], _columns[j]);

                        _protocol.SaveTable(MatrixA, _rows, _columns);

                        optimal = false;
                        break;
                    }
                }
            }

            double[] resX = GenerateResult(MatrixA);

            resX[resX.Length-1] = MatrixA[rowsCount, colsCount];

            return resX;
        }



        public int FindMinPositiveRatio(double[,] MatrixA, int column)
        {
            
            double temp = 0;
            double pivot = double.MaxValue;
            int row = -1;

            int rowsCount = MatrixA.GetLength(0) - 1;
            int colsCount = MatrixA.GetLength(1) - 1;


            for (int i = 0; i < rowsCount; i++)
            {
                if (pivot > 0)
                {
                    temp = pivot;
                }


                if (MatrixA[i, column] != 0)
                {
                    pivot = MatrixA[i, colsCount] / MatrixA[i, column];

                    if (MatrixA[i, colsCount] == 0 && MatrixA[i, column] > 0)
                    {
                        row = i;
                        break;
                    }

                }



                if (pivot > 0 && pivot < temp)
                {
                    temp = pivot;
                    row = i;
                }
            }
            return row;
        }




        public void InitializeLabels(int rows, int cols)
        {

            _rows = new string[rows+1];

            for (int i = 0; i < rows; i++)
            {
                _rows[i] = "y" + (i + 1);
            }

            _rows[rows] = "Z";
            
            _columns = new string[cols+1];
            for (int j = 0; j < cols; j++)
            {
                _columns[j] = "x" + (j + 1);
            }
            _columns[cols] = "1";           
        }


        private double[] GenerateResult(double[,] MatrixA)
        {
            int rowsCount = MatrixA.GetLength(0) - 1;
            int colsCount = MatrixA.GetLength(1) - 1;

            double[] x = new double[colsCount+1];


            for (int i = 0; i < rowsCount; i++)
            {
                string name = _rows[i];

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
