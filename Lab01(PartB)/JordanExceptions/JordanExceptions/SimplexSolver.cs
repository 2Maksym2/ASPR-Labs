using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class SimplexSolver
    {
        private string _rows { get; set; }
        private string _columns { get; set; }
        private readonly ISaveProtocol _protocol;
        private readonly IJordanMethod _jordan;

        public SimplexSolver(SaveProtocol protocol, JordanMethod jordan) 
        { 
            _protocol = protocol;            
            _jordan = jordan;
        }



        public void FindInitialFeasibleSolution(double[,] MatrixA)
        {
            int row = -1;
            int column = -1;
            double pivot = 0;

            bool IsComplete = false;

            

            for (int i = 0; i < MatrixA.GetLength(0)-1; i++)
            {
                if (MatrixA[i, MatrixA.GetLength(1)-1] < 0)
                {
                    row = i;
                    IsComplete = true;
                    ;
                }                
            }



            if (IsComplete)
            {
                IsComplete = false;
                
                for (int j = 0; j < MatrixA.GetLength(1)-1; j++)
                {
                    if (MatrixA[row, j] < 0)
                    {
                        column = j;
                        IsComplete = true;
                        continue;
                    }
                }


                if (IsComplete)
                {





                    MatrixA = _jordan.MatrixSolver(MatrixA, pivot, row, column);
                }
                else
                {
                    throw new Exception("Система обмежень ЛП-задачі суперечлива");
                }
                   
                            
            }
        }                 
          
        

        public void FindOptimalSolution()
        {

        }
    }
}
