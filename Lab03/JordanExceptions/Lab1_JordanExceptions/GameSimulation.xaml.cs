using JordanExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Lab1_JordanExceptions
{
    public partial class GameSimulation : Window
    {
        public GameSimulation(double[,] originalMatrix, double[] p, double[] q, int n)
        {
            InitializeComponent();
            RunSimulation(originalMatrix, p, q, n);
        }

        private void RunSimulation(double[,] matrix, double[] p, double[] q, int n)
        {
            Random rng = new Random();
            List<SimulationRow> history = new List<SimulationRow>();
            double totalWin = 0;

            
            int[] countA = new int[p.Length];
            int[] countB = new int[q.Length];

            for (int i = 1; i <= n; i++)
            {
                
                double rA = Math.Round(rng.NextDouble(), 3);
                double rB = Math.Round(rng.NextDouble(), 3);

               
                int indexA = SelectStrategy(p, rA);
                int indexB = SelectStrategy(q, rB);

                
                double currentWin = matrix[indexA, indexB];
                totalWin += currentWin;

                countA[indexA]++;
                countB[indexB]++;

                history.Add(new SimulationRow
                {
                    Id = i,
                    RandA = rA,
                    StrategyA = $"X{indexA + 1}",
                    RandB = rB,
                    StrategyB = $"Y{indexB + 1}",
                    WinA = currentWin,
                    TotalWinA = totalWin,
                    AvgWinA = Math.Round(totalWin / i, 3)
                });
            }

            dgSimulation.ItemsSource = history;

            txtFinalP.Text = string.Join("  ", countA.Select(c => ((double)c / n).ToString("F2")));
            txtFinalQ.Text = string.Join("  ", countB.Select(c => ((double)c / n).ToString("F2")));
            txtFinalV.Text = (totalWin / n).ToString("F2");
        }

        private int SelectStrategy(double[] probs, double r)
        {
            double sum = 0;
            for (int i = 0; i < probs.Length; i++)
            {
                sum += probs[i];
                if (r <= sum) return i;
            }
            return probs.Length - 1;
        }
    }
}