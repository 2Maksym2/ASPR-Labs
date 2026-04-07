using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class RankSolver
    {
        private readonly ISaveProtocol _protocol;
        private readonly IJordanMethod _jordan;

        public RankSolver(ISaveProtocol protocol, IJordanMethod jordan) 
        { 
            _protocol = protocol;
          _jordan = jordan;
        }

        public int GetSolution(double[,] matrixA, int r, int c) 
        {
            int rows = matrixA.GetLength(0);
            int cols = matrixA.GetLength(1);
            double[,] matrixB = matrixA;


            int rank = 0;
           
            int n = Math.Min(r, c);

            {            
                for (int i = 0; i < n; i++)           
                {
                    double pivot = matrixB[i, i];
                    if (pivot != 0)
                    {
                        matrixB = _jordan.MatrixSolver(matrixB, pivot, i, i);
                        rank++;
                        _protocol.RankSave(rank, matrixA, matrixB);
                    }
                }
            }
           
            _protocol.RankResultSave(rank, matrixB);
            return rank;
        }
    }
}
