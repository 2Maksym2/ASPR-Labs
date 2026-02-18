using JordanExceptions;

namespace TestProject
{
    public class LinearSolverTest
    {
        [Fact]
        public void GetSolution()
        {
            var _jordan = new JordanMethod();
            var _solver = new LinearSolver();
            var _invertor = new MatrixInvertor(_jordan);

            double[,] inputMatrix = {
              { 6, 2, 5},
              {-3, 4, -1},
              {1, 4, 3}
              };

            inputMatrix = _invertor.InvertMatrix(inputMatrix);
            double[] a = {1, 6, 6};
            int count = a.Length;
            double[] expected = { -1, 1, 1 };

            a = _solver.GetSolution(inputMatrix, a);

                for (int i = 0; i < count; i++)
                {
                    Assert.Equal(expected[i], a[i], precision: 2);
                }
        }
    }
}