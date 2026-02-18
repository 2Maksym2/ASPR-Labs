namespace JordanExceptions


{
    public class JordanMethod
    {

        public JordanMethod() 
        { 
        }


        public double[,] MatrixSolver(double[,] matrixA, double a, int n)
        {
            int rows = matrixA.GetLength(0);
            int cols = matrixA.GetLength(1);
            double[,] matrixB = new double[rows, cols];

            matrixB[n, n] = 1 / a;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (i == n && j == n) continue;

                    if (i == n && j != n)
                        matrixB[n, j] = -matrixA[n, j] / a;

                    if (i != n && j == n)                  
                        matrixB[i, n] = matrixA[i, n] / a;
                  
                    matrixB[i, j] = (a * matrixA[i, j] - matrixA[i, n] * matrixA[n, j]) / a;                   
                }
            }


            return matrixB;

        }
    }
}
