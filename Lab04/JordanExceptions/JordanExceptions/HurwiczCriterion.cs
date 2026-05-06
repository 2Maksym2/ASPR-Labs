using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class HurwiczCriterion
    {
        public HurwiczCriterion()
        {

        }


        public List<int> Solver(double[,] matrix, double coef)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double max = double.MinValue;
            List<int> resultRow;
            double[] maxInRows = new double[rows];
            double min = double.MaxValue;
            double[] minInRows = new double[rows];
            double[] resInRows = new double[rows];

            for (int i = 0; i < rows; i++)
            {
                min = matrix[i, 0];
                for (int j = 1; j < cols; j++)
                {
                    if (matrix[i, j] < min) min = matrix[i, j];
                }
                minInRows[i] = min;
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

            for (int i = 0; i < rows; i++)
            {
               resInRows[i] = minInRows[i]*coef + (1-coef)*maxInRows[i];
            }


            double a = resInRows.Max();
            return resultRow = resInRows.Select((value, index) => new { value, index })
                                        .Where(x => x.value == a)
                                        .Select(x => x.index)
                                        .ToList();
        }
    }
}
