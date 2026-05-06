using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class MaximaxCriterion
    {
        public MaximaxCriterion()
        {

        }


        public List<int> Solver(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double max = double.MinValue;
            List<int> resultRow;
            double[] maxInRows = new double[rows];

            for (int i = 0; i < rows; i++)
            {
                max = matrix[i, 0];
                for (int j = 1; j < cols; j++)
                {
                    if (matrix[i, j] > max) max = matrix[i, j];
                }
                maxInRows[i] = max;
            }

            double a = maxInRows.Max();
            return resultRow = maxInRows.Select((value, index) => new { value, index })
                                        .Where(x => x.value == a)
                                        .Select(x => x.index)
                                        .ToList();
        }
    }
}
