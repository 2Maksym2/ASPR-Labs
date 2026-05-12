using JordanExceptions;

namespace TestProject
{
    public class LinearSolverTest
    {
        [Theory]
        [InlineData(new double[] { 5, -3, 7, -1, 4, 3, 6, -2, 5 }, 3, new double[] { 13, 13, 12 }, new double[] { 1, 2, 2 })]
        [InlineData(new double[] {6,2,5,-3,4,-1,1,4,3 }, 3, new double[] { 1, 6, 6 }, new double[] { -1,1,1 })]
        [InlineData(new double[] {-1,1,1,-1,-2,2,3,-1,3}, 3, new double[] {4,3,2}, new double[] {-1,1,2})]
        public void GetSolution(double[] Matrix, int n, double[] a, double[] expected)
        {
            var _jordan = new JordanMethod();
            var _protocol = new SaveProtocol();
            var _invertor = new MatrixInvertor(_jordan, _protocol);
            var _solver = new LinearSolver(_protocol, _invertor);


            double[,] inputMatrix = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    inputMatrix[i, j] = Matrix[i * n + j];

            int count = a.Length;



            a = _solver.GetSolution(inputMatrix, a);

                for (int i = 0; i < count; i++)
                {
                    Assert.Equal(expected[i], a[i], precision: 2);
                }
        }
    }
}