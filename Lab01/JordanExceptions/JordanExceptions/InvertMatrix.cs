namespace JordanExceptions
{
    public class MatrixInvertor : IMatrixInvertor
    {
        private readonly IJordanMethod _jordan;
        public MatrixInvertor(IJordanMethod jordan) 
        { 
         _jordan = jordan;
        }

        public double[,] InvertMatrix(double[,] matrixA)
        {
            int n = matrixA.GetLength(0);
            double[,] result = matrixA;
            for (int i = 0; i < n; i++)
            {
                result = _jordan.MatrixSolver(result, result[i, i], i, i);
            }

            return result;
        }
    }
}