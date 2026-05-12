using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class WaldCriterion
    {
        protected readonly ISaveProtocol _protocol;

        public int RowsEqualityCount { get; set; }
        public WaldCriterion(ISaveProtocol protocol)
        {
            _protocol = protocol;
        }


        public List<int> Solver(double[,] matrix) 
        { 
          int rows = matrix.GetLength(0);
          int cols = matrix.GetLength(1);
          double min = double.MaxValue;
          List<int> resultRow;
          double[]  minInRows = new double[rows];
            _protocol.SaveSectionHeader("Критерій Вальда");

            for (int i = 0; i < rows; i++)
            {
                min = matrix[i, 0];
                for (int j = 1; j < cols; j++)
                {
                    if (matrix[i, j] < min) min = matrix[i, j];
                }
                minInRows[i] = min;
                _protocol.StepSave($"\nМінімум в рядку {i+1} = {min}");

            }

            double a = minInRows.Max();
            _protocol.StepSave($"\nМаксимальний елемент = {a}");

            return resultRow = minInRows.Select((value, index) => new { value, index })
                                        .Where(x => x.value == a)    
                                        .Select(x => x.index)    
                                        .ToList();
        }
    }
}
