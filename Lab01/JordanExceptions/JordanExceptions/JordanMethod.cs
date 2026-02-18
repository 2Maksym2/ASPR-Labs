namespace JordanExceptions


{
    public class JordanMethod : IJordanMethod
    {

        public JordanMethod()
        {
        }


        public double[,] MatrixSolver(double[,] matrixA, double a, int r, int s)
        {
            int rows = matrixA.GetLength(0);
            int cols = matrixA.GetLength(1);
            double[,] matrixB = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (i == r && j == s) matrixB[r, s] = 1 / a;

                    else if (i == r && j != s)
                        matrixB[r, j] = -matrixA[r, j] / a;

                    else if (i != r && j == s)
                        matrixB[i, s] = matrixA[i, s] / a;

                    else
                    {
                        matrixB[i, j] = (a * matrixA[i, j] - matrixA[i, s] * matrixA[r, j]) / a;
                    }
                }
            }


            return matrixB;

        }
    }
}
