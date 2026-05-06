using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class SavageCriterion
    {

        protected readonly ISaveProtocol _protocol;

        public int RowsEqualityCount { get; set; }
        public SavageCriterion(ISaveProtocol protocol)
        {
            _protocol = protocol;
        }


        public List<int> Solver(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double max = double.MinValue;
            List<int> resultRow;
            double[] maxInRows = new double[rows];
            double[] maxInCols = new double[cols];
            double[,] newMatrix = matrix;

            _protocol.SaveSectionHeader("Критерія Севіджа");
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
                    newMatrix[i,j] = maxInCols[j] - matrix[i, j];
                }
            }
            _protocol.StepSave("\nМатриця ризиків:");

            _protocol.SaveMatrix(newMatrix);

            for (int i = 0; i < rows; i++)
            {
                max = newMatrix[i, 0];
                for (int j = 1; j < cols; j++)
                {
                    if (newMatrix[i, j] > max) max = newMatrix[i, j];
                }
                maxInRows[i] = max;
                _protocol.StepSave($"\nМаксимум в рядку {i+1} = {max}");

            }

            double a = maxInRows.Min();
            _protocol.StepSave($"\nМінімальний елемент = {a}");

            return resultRow = maxInRows.Select((value, index) => new { value, index })
                                        .Where(x => x.value == a)
                                        .Select(x => x.index)
                                        .ToList();
        }

    }
}
