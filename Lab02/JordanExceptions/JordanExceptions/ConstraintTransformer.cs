using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class ConstraintTransformer
    {
        private readonly ISaveProtocol _protocol;
        private readonly IJordanMethod _jordan;
        private readonly MinimalPositiveRatioFinder _minRatio;

        public int RowsEqualityCount { get; set; }
        public ConstraintTransformer(ISaveProtocol protocol, IJordanMethod jordan, SimplexSolver solver)
        {
            _protocol = protocol;
            _jordan = jordan;
            _minRatio = new MinimalPositiveRatioFinder();
        }

        public double[,] EliminateEqualities(int col, double[,] MatrixA, ref string[] cols, string[] rows)
        {
            int rowsCount = MatrixA.GetLength(0);
            int colsCount = MatrixA.GetLength(1);

            double[,] MatrixB = new double[rowsCount, colsCount-1];


            for (int i = 0; i < rowsCount; i++)
            {
                int J = 0;
                for (int j = 0; j < colsCount; j++)
                {
                    if (j == col) continue;
                    MatrixB[i, J] = MatrixA[i, j];
                    J++;
                }
            }

             cols = cols.Where((x, id) => id!= col).ToArray();

            _protocol.SaveSectionHeader("Змінена матриця");
            _protocol.SaveTable(MatrixB, rows, cols);


            return MatrixB;
        }


        public double[,] ToCanonicalForm(double[,] MatrixA, ref string[] _columns, ref string[] _rows)
        {
            while (true)
            {
               
                if (_rows.Contains("0"))
                {
                    int colsCount = MatrixA.GetLength(1) -1;
                    int r = Array.IndexOf(_rows, "0");

                    int column = -1;

                    for (int j = 0; j < colsCount; j++)
                    {
                        if (MatrixA[r, j] > 0)
                        {
                            column = j;
                            break;
                        }
                    }


                    if (column == -1)
                    {
                        throw new Exception("Система обмежень суперечлива (немає опорного розв'язку)");
                    }


                    r = _minRatio.FindMinPositiveRatio(MatrixA, column);
                    
                    if (r < 0) throw new Exception("Функція не має максимуму/мінімуму");

                    double pivot = MatrixA[r, column];

                    _protocol.SaveSectionHeader("Видалення рівностей з симплекс таблиці");


                    (_columns[column], _rows[r]) = (_rows[r], _columns[column]);

                    MatrixA = _jordan.MatrixSolver(MatrixA, pivot, r, column);

                    _protocol.SaveSectionHeader("Матриця після Жорданових виключень");
                    _protocol.SaveTable(MatrixA, _rows, _columns);


                    if (_columns[column] == "0")
                    {
                        MatrixA = EliminateEqualities(column, MatrixA, ref _columns, _rows);
                    }
                }

                else return MatrixA;                
            }
        }

    }
}
