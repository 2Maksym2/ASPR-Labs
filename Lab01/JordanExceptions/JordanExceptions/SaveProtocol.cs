using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class SaveProtocol : ISaveProtocol
    {
        private readonly string _path = "Protocol.txt";
        public SaveProtocol()
        {
        }

        public void FileCleaner()
        {
            lock (_path)
            {

                File.WriteAllText(_path, "ЗГЕНЕРОВАНИЙ ПРОТОКОЛ ОБЧИСЛЕНЬ \n\n\n");
            }
        }


        public void InputMatrix(double[,] b)
        {
            lock (_path)
            {

                string res = "\nВхідна матриця: \n";
                for (int i = 0; i < b.GetLength(0); i++)
                {
                    res += "\n";
                    for (int j = 0; j < b.GetLength(1); j++)
                    {
                        res += $" {b[i, j]:F2}";
                    }
                }
                res+= "\n";
                File.AppendAllText(_path, res);
            }

        }

        public void ResultSave(int stepNumber, double[,] x, double[,] a)
        {
            lock (_path)
            {
                string res = $"\nКрок {stepNumber+1}:  \n";
                res += $"\nРозв`язувальний елемент A[{stepNumber}][{stepNumber}]: {a[stepNumber, stepNumber]:F2} \n";
                res += "\nМатриця після виконання ЗЖВ:  \n";

                for (int i = 0; i < x.GetLength(0); i++)
                {
                    for (int j = 0; j < x.GetLength(1); j++)
                    {
                        res += $" {x[i, j]:F2}";
                    }
                    res += "\n";
                }

                File.AppendAllText(_path, res);

            }
        }

        public void InvertMatrixSave(double[,] b)
        {
            lock (_path)
            {

                string res = "\n Обернена матриця:  \n";

                for (int i = 0; i < b.GetLength(0); i++)
                {
                    for (int j = 0; j < b.GetLength(1); j++)
                    {
                        res += $" {b[i, j]:F2}";
                    }
                    res += "\n";

                }


                File.AppendAllText(_path, res);
            }

        }

        public void StepSave(string res)
        {
            lock (_path)
            {

                File.AppendAllText(_path, res);
            }
        }

        public void RankSave(int r, double[,] x, double[,] a)
        {
            lock (_path)
            {

                string res = "\n Початкова матриця:  \n";

                for (int i = 0; i < x.GetLength(0); i++)
                {
                    for (int j = 0; j < x.GetLength(1); j++)
                    {
                        res += $" {x[i, j]:F2}";
                    }
                    res += "\n";

                }
                res += "\n Матриця після виконання ЗЖВ:  \n";

                for (int i = 0; i < a.GetLength(0); i++)
                {
                    for (int j = 0; j < a.GetLength(1); j++)
                    {
                        res += $" {a[i, j]:F2}";
                    }
                    res += "\n";

                }

                res += $"\n Поточний ранг R = {r} \n";
                File.AppendAllText(_path, res);
            }
        }

        public void RankResultSave(int r, double[,] x)
        {
            lock (_path)
            {

                string res = "\n Кінцева матриця:  \n";

                for (int i = 0; i < x.GetLength(0); i++)
                {
                    for (int j = 0; j < x.GetLength(1); j++)
                    {
                        res += $" {x[i, j]:F2}";
                    }
                    res += "\n";

                }


                res += $"Ранг матриці R = {r}";
                File.AppendAllText(_path, res);
            }
        }

    }
}
