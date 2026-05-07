using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class MatrixAnalyzer
    {
        protected readonly ISaveProtocol _protocol;
        public List<int> InactiveRows { get; private set; } = new List<int>();
        public List<int> InactiveCols { get; private set; } = new List<int>();

        public MatrixAnalyzer(ISaveProtocol protocol)
        {
            _protocol = protocol;
        }
        public double[] PureStrategy(double[,] matrixA)
        {
            int columns = matrixA.GetLength(1);
            int rows = matrixA.GetLength(0);
            double resultRow;
            double resultColumn;

            _protocol.InputMatrix(matrixA);

            double[] result = new double[3];
            double[] minInRows = new double[rows];
            double[] maxInColumns = new double[columns];
          
            for (int i = 0; i < rows; i++)
            {
                double min = matrixA[i, 0];
                for (int j = 1; j < columns; j++)
                {
                    if (matrixA[i, j] < min) min = matrixA[i, j];
                }
                minInRows[i] = min;
            }


            for (int j = 0; j < columns; j++)
            {
                double max = matrixA[0, j];
                for (int i = 1; i < rows; i++)
                {
                    if (matrixA[i, j] > max) max = matrixA[i, j];
                }
                maxInColumns[j] = max;
            }



            double a = minInRows.Max();
            _protocol.SaveSectionHeader($"Нижня ціна гри: {a}");

            resultRow = Array.IndexOf(minInRows, a);

            double b = maxInColumns.Min();
            _protocol.SaveSectionHeader($"Верхня ціна гри: {b}");

            resultColumn = Array.IndexOf(maxInColumns, b);

            if (a == b)
            {
                result[0] = resultRow;
                result[1] = resultColumn;
                result[2] = a;


                return result;
            }
            else return null;
         }



        public double[,] SimplifyMatrix(double[,] matrix)
        {
            InactiveRows.Clear();
            InactiveCols.Clear();

            bool changed;
            double[,] currentMatrix = matrix;

            do
            {
                changed = false;

                int rowsBefore = currentMatrix.GetLength(0);
                currentMatrix = RemoveDominatedRows(currentMatrix);
                if (currentMatrix.GetLength(0) < rowsBefore) changed = true;

                int colsBefore = currentMatrix.GetLength(1);
                currentMatrix = RemoveDominatedColumns(currentMatrix);
                if (currentMatrix.GetLength(1) < colsBefore) changed = true;

            } while (changed);

            return currentMatrix;
        }

        private double[,] RemoveDominatedRows(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            List<int> survivors = new List<int>();
            for (int i = 0; i < rows; i++) survivors.Add(i);
            List<int> toRemove = new List<int>();

            for (int i = 0; i < rows; i++)
            {
                if (toRemove.Contains(i)) continue;

                for (int j = 0; j < rows; j++)
                {
                    if (i == j || toRemove.Contains(j)) continue;

                    if (IsRowADominatingB(matrix, i, j, cols))
                    {
                        toRemove.Add(j);
                        InactiveRows.Add(j);
                    }
                }
            }

            survivors.RemoveAll(idx => toRemove.Contains(idx));

            return RebuildMatrix(matrix, survivors, null);
        }


        private double[,] RemoveDominatedColumns(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            List<int> activeCols = new List<int>();
            for (int j = 0; j < cols; j++) activeCols.Add(j);

            for (int i = 0; i < activeCols.Count; i++)
            {
                for (int j = 0; j < activeCols.Count; j++)
                {
                    if (i == j) continue;

                    if (IsColumnADominatingB(matrix, activeCols[i], activeCols[j], rows))
                    {
                        InactiveCols.Add(activeCols[j]);
                        activeCols.RemoveAt(j);
                        j--;
                    }
                }
            }
            return RebuildMatrix(matrix, null, activeCols);
        }

        private bool IsRowADominatingB(double[,] matrix, int rowA, int rowB, int cols)
        {
            for (int c = 0; c < cols; c++)
                if (matrix[rowA, c] < matrix[rowB, c]) return false;
            return true;
        }

        private bool IsColumnADominatingB(double[,] matrix, int colA, int colB, int rows)
        {
            for (int r = 0; r < rows; r++)
                if (matrix[r, colA] > matrix[r, colB]) return false;
            return true;
        }

        private double[,] RebuildMatrix(double[,] oldMatrix, List<int> rows, List<int> cols)
        {
            int newRows = rows?.Count ?? oldMatrix.GetLength(0);
            int newCols = cols?.Count ?? oldMatrix.GetLength(1);
            double[,] result = new double[newRows, newCols];

            for (int r = 0; r < newRows; r++)
                for (int c = 0; c < newCols; c++)
                    result[r, c] = oldMatrix[rows?[r] ?? r, cols?[c] ?? c];

            return result;
        }
    }
}
