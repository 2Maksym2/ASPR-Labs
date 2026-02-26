using JordanExceptions;

namespace TestProject
{
    public class SimplexSolverTest
    {
        [Theory]
        [InlineData(new double[] {1,1,-1,-2,6,-1,-1,-1,1,-5,2,-1,3,4,10,-1,-2,1,1,0  }, 4, 5, new double[] { 0,22,0,8 }, 36)]
        public void GetSolution(double[] Matrix, int r, int c, double[] expectedX, int expectedResult)
        {
            var _jordan = new JordanMethod();
            var _protocol = new SaveProtocol();
            var _simplex = new SimplexSolver(_protocol, _jordan);
            double R = 0;
            _protocol.FileCleaner();

            double[,] inputMatrix = new double[r, c];
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    inputMatrix[i, j] = Matrix[i * c + j];


            double[] X = _simplex.FindOptimalSolution(inputMatrix);

            for (int i = 0; i < X.Length; i++)
            {
                R += X[i] * -inputMatrix[r-1,i];
            }
            

            for (int i = 0; i < X.Length; i++)
            {
                Assert.Equal(expectedX[i], X[i], precision: 2);
            }
           
            Assert.Equal(expectedResult, R, precision: 2);

        }
    }
}