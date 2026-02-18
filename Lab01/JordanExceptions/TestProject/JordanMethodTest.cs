using JordanExceptions;

namespace TestProject
{
    public class JordanMethodTest
    {
        [Fact]
        public void MatrixSolverTest()
        {
            var jordan = new JordanMethod();
            var solver = new MatrixInvertor(jordan);

            double[,] inputMatrix = {
              { 5.0, -3.0, 7.0 },
              { -1.0, 4.0, 3.0 },
              { 6.0 , -2.0 , 5.0 }
              };

            int col = inputMatrix.GetLength(0);

            double[,] expectedMatrix = {
            { -0.28, -0.011,  0.398 },
            { -0.247,  0.183,  0.237 },
            {  0.237,  0.086, -0.183 }
};
              
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