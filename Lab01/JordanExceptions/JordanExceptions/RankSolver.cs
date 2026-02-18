using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class RankSolver
    {
        private readonly IJordanMethod _jordan;
        public RankSolver(IJordanMethod jordan) 
        { 
          _jordan = jordan;
        }



        public int GetSolution(double[,] matrixA, int r, int c) 
        {


            int rank = 0;
           
            int limit = Math.Min(r, c);

            {            
                for (int i = 0; i < limit; i++)           
                {
                    double pivot = matrixA[i, i];
                    if (pivot != 0)
                    {
                        matrixA = _jordan.MatrixSolver(matrixA, pivot, i, i);
                        rank++;
                    }
                }
            }
           

            return rank;
        }
    }
}
