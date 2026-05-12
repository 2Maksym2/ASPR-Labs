using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class LaplaceCriterion
    {
        protected readonly ISaveProtocol _protocol;

        public int RowsEqualityCount { get; set; }
        public LaplaceCriterion(ISaveProtocol protocol)
        {
            _protocol = protocol;
        }


        public List<int> Solver(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            List<int> resultRow;
            double[] resInRow = new double[rows];
            _protocol.SaveSectionHeader("Критерій Лапласа");

            for (int i = 0; i < rows; i++)
            {
                double sum = 0;
                for (int j = 0; j < cols; j++)
                {
                    sum += matrix[i, j];
                }
                resInRow[i] = sum / (double)cols;
                _protocol.StepSave($"\nS{i+1} = {resInRow[i]:F2}");
            }


            double a = resInRow.Max();
            _protocol.StepSave($"\nМаксимальний елемент = {a}");

            return resultRow = resInRow.Select((value, index) => new { value, index })
                                        .Where(x => x.value == a)
                                        .Select(x => x.index)
                                        .ToList();
        }
    }
}
