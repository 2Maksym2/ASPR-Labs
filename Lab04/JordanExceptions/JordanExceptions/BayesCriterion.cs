using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class BayesCriterion
    {

        public BayesCriterion()
        {

        }


        public List<int> Solver(double[,] matrix, double[] strategy)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            List<int> resultRow;
            double[] resInRow = new double[rows];

            for (int i = 0; i < rows; i++)
            {
                double sum = 0;

                for (int j = 0; j < cols; j++)
                {
                    sum += matrix[i, j] * strategy[j];
                }              
                resInRow[i] = sum;
            }


            double a = resInRow.Max();

            return resultRow = resInRow.Select((value, index) => new { value, index })
                                        .Where(x => x.value == a)
                                        .Select(x => x.index)
                                        .ToList();
        }

    }
}
