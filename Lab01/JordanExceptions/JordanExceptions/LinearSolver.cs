using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class LinearSolver
    {
        private readonly ISaveProtocol _protocol;

        public LinearSolver(ISaveProtocol protocol) 
        { 
          _protocol = protocol;
        }

        public double[] GetSolution(double[,] MatrixA, double[] a) 
        {
            int x = a.Length;
            double [] b = new double[x];

            _protocol.StepSave("Вхідна матриця B: \n\n");
            for (int i = 0; i < b.Length; i++) _protocol.StepSave($"{ a[i]} \n");

            for (int i = 0; i < x; i++) 
            {
                string res = $"\nX{i}:";

                for (int j = 0; j < x; j++) 
                {
                    b[i] += a[j] * MatrixA[i,j];



                    res += $"{a[j]:F2}  *  {MatrixA[i, j]:F2}";
                    if (j < x - 1)
                        res += "  +  ";
                    else res += $"  = {b[i]:F1}  \n" ;
                }
                
                
                _protocol.StepSave(res);

            }
            return b;
        }
    }
}
