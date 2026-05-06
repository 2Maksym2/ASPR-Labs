using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class MaximaxCriterion
    {
        protected readonly ISaveProtocol _protocol;

        public int RowsEqualityCount { get; set; }
        public MaximaxCriterion(ISaveProtocol protocol)
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

            _protocol.SaveSectionHeader("Критерій максимаксу");

            for (int i = 0; i < rows; i++)
            {
                max = matrix[i, 0];
                for (int j = 1; j < cols; j++)
                {
                    if (matrix[i, j] > max) max = matrix[i, j];
                }
                maxInRows[i] = max;
                _protocol.StepSave($"\nМаксимум в рядку {i + 1} = {max}");

            }

            double a = maxInRows.Max();
            _protocol.StepSave($"\nМаксимальний елемент = {a}");

            return resultRow = maxInRows.Select((value, index) => new { value, index })
                                        .Where(x => x.value == a)
                                        .Select(x => x.index)
                                        .ToList();
        }
    }
}
