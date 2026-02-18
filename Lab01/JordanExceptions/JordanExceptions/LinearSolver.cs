using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class LinearSolver
    {
        public LinearSolver() { }

        public double[] GetSolution(double[,] MatrixA, double[] a) 
        {
            int x = a.Length;
            double [] b = new double[x];
            for (int i = 0; i < x; i++) 
            {
                for (int j = 0; j < x; j++) 
                {
                    b[i] += a[j] * MatrixA[i,j];
                } 
            }
            return b;
        }
    }
}
