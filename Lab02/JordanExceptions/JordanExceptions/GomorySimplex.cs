using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class GomorySimplex : SimplexSolver
    {
        public GomorySimplex(ISaveProtocol protocol, IJordanMethod jordan) : base(protocol, jordan) { }

        public double[] Solver(double[,] MatrixA)
        {
            Rows = null;
            Columns = null;
            LastMatrix = null;

            _protocol.SaveSectionHeader("ЦІЛОЧИСЕЛНЕ РОЗВ'ЯЗАННЯ: ");


            int row = 0;
            double epsilon = 1e-9;
            while (true)
            {
                double[] fullSolution = FindOptimalSolution(MatrixA);
                double[] x = fullSolution.Take(fullSolution.Length - 1).ToArray();



                MatrixA = LastMatrix;

                for (int i = 0; i < x.Length-1; i++)
                {
                    if (Math.Abs(x[i] - Math.Round(x[i])) > epsilon)
                    {
                        double[] Constraints = GenerateGomoryCut(FindMaxFractionalRow(x));
                        MatrixA = AddConstraintToTable(MatrixA, Constraints);
                        continue;
                    }
                    else return fullSolution;
                }


            }                        
        }


        public int FindMaxFractionalRow(double[] x)
        {
            int row = 0;
            double temp = double.MinValue;
           
            for (int i = 0; i < x.Length-1; i++)
            {
                if (x[i]%1 > temp)
                {
                    temp = x[i] % 1;
                    row = Array.FindIndex(Rows, x => x == "x" + (i+1));
                }
            }


            return row;
        }

        public double[] GenerateGomoryCut(int row)
        {
            int cols = LastMatrix.GetLength(1);
            double[] Constraints = new double[cols];


            for (int i = 0; i < cols; i++)
            {
                double val = LastMatrix[row, i];
                Constraints[i] = -(val - Math.Floor(val));
            }

            return Constraints;

        }

        public double[,] AddConstraintToTable(double[,] MatrixA, double[] Constraints)
        {

            int rows = LastMatrix.GetLength(0);
            int cols = LastMatrix.GetLength(1);

            double[,] newMatrix = new double[rows+1, cols];

            for (int i = 0; i < rows + 1; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (i < rows-1)
                    {
                        newMatrix[i, j] = LastMatrix[i, j];
                    }

                    else if (i == rows-1)
                    {
                        newMatrix[i, j] = Constraints[j];
                    }

                    else
                    {
                        newMatrix[i, j] = LastMatrix[i - 1, j];
                    }
                }
            }

            string[] newLabels = new string[rows + 1];

            for (int i = 0; i < rows - 1; i++)
            {
                newLabels[i] = Rows[i];
            }

            newLabels[rows - 1] = "S";

            newLabels[rows] = "Z";

            Rows = newLabels;
            return newMatrix;
        }
    }
}
