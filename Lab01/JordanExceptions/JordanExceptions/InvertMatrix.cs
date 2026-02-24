using System.Reflection.Metadata.Ecma335;

namespace JordanExceptions
{
    public class MatrixInvertor : IMatrixInvertor
    {
        private readonly IJordanMethod _jordan;
        private readonly ISaveProtocol _protocol;
        public MatrixInvertor(IJordanMethod jordan, ISaveProtocol protocol) 
        {          
            _jordan = jordan;
            _protocol = protocol;
        }

        public double[,] InvertMatrix(double[,] matrixA)
        {
            int n = matrixA.GetLength(0);
            double[,] result = matrixA;

            _protocol.InputMatrix(matrixA);

            for (int i = 0; i < n; i++)
            {
                double[,] log = result;
                result = _jordan.MatrixSolver(result, result[i, i], i, i);
                _protocol.ResultSave(i, result, log);
            }

            _protocol.InvertMatrixSave(result);
            return result;
        }
    }
}