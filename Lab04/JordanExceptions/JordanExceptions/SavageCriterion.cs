using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class SavageCriterion
    {

        public SavageCriterion()
        {

        }


        public List<int> Solver(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double max = double.MinValue;
            List<int> resultRow;
            double[] maxInRows = new double[rows];
            double[] maxInCols = new double[cols];

            for (int j = 0; j < cols; j++)
            {
                max = matrix[0, j];
                for (int i = 1; i < rows; i++)
                {
                    if (matrix[i, j] > max) max = matrix[i, j];
                }
                maxInCols[j] = max;
            }

            for (int j = 0; j < cols; j++)
            {
                for (int i = 0; i < rows; i++)
                {
                    matrix[i,j] = maxInCols[j] - matrix[i, j];
                }
            }


            for (int i = 0; i < rows; i++)
            {
                max = matrix[i, 0];
                for (int j = 1; j < cols; j++)
                {
                    if (matrix[i, j] > max) max = matrix[i, j];
                }
                maxInRows[i] = max;
            }

            double a = maxInRows.Min();

            return resultRow = maxInRows.Select((value, index) => new { value, index })
                                        .Where(x => x.value == a)
                                        .Select(x => x.index)
                                        .ToList();
        }

    }
}
