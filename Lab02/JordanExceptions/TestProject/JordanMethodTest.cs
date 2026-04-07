using JordanExceptions;

namespace TestProject
{
    public class JordanMethodTest
    {
        [Theory]
        [InlineData(new double[] {5,-3,7,-1,4,3,6,-2,5  }, 3, new double[] { -0.28, -0.011, 0.398, -0.247, 0.183, 0.237, 0.237, 0.086, -0.183 })]
        [InlineData(new double[] {6,2,5,-3,4,-1,1,4,3  }, 3, new double[] {0.5, 0.437, -0.687, 0.25, 0.406, -0.281, -0.5, -0.687, 0.937 })]
        [InlineData(new double[] {2,-1,3,-1,2,2,1,1,1  }, 3, new double[] {0, -0.333, 0.667, -0.25, 0.083, 0.583, 0.25, 0.25, -0.25  })]

        public void MatrixSolverTest(double[] Matrix, int n, double[] Expected)
        {
            var _protocol = new SaveProtocol();
            var jordan = new JordanMethod();
            var solver = new MatrixInvertor(jordan, _protocol);

            double[,] inputMatrix = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    inputMatrix[i, j] = Matrix[i * n + j];

            int col = inputMatrix.GetLength(0);

            double[,] expectedMatrix = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    expectedMatrix[i, j] = Expected[i * n + j];
        
              
            inputMatrix = solver.InvertMatrix(inputMatrix);

            for (int i = 0; i < col; i++) {
                for (int j = 0; j < col; j++)
                {
                    Assert.Equal(expectedMatrix[i, j], inputMatrix[i, j], precision: 2);
                } 
            }
        }
    }
}