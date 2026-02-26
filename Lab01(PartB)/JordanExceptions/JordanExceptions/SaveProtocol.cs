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


                res += $"\n Ранг матриці R = {r}";
                File.AppendAllText(_path, res);
            }
        }


        private string FormatSimplexTable(double[,] matrix, string[] rows, string[] columns)
        {            
            StringBuilder sb = new StringBuilder();
            int rCount = matrix.GetLength(0);
            int cCount = matrix.GetLength(1);

            sb.Append("\t");
            for (int j = 0; j < cCount; j++)
            {
                string label = columns[j];
                string header = (label == "1" || label == "Z") ? label : "-" + label;
                sb.Append($"{header}\t");
            }
            sb.AppendLine("\n" + new string('-', 50));

            for (int i = 0; i < rCount; i++)
            {
                string rowLabel = rows[i];
                sb.Append($"{rowLabel} =\t");
                for (int j = 0; j < cCount; j++)
                {
                    sb.Append($"{matrix[i, j]:F2}\t");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }



        public void InputTableSave(double[,] matrix, string[] rows, string[] columns)
        {
            lock (_path)
            {
                File.AppendAllText(_path, "\nВхідна симплекс-таблиця:\n\n");
                File.AppendAllText(_path, FormatSimplexTable(matrix, rows, columns));
                File.AppendAllText(_path, "\n" + new string('=', 50) + "\n");
            }
        }




        public void SaveStepHeader(string rowName, string colName, string title = "")
        {
            lock (_path)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"\n{title}:");
                sb.AppendLine($"Розв’язувальний рядок:   {rowName}");
                sb.AppendLine($"Розв’язувальний стовпець: -{colName}\n");
                File.AppendAllText(_path, sb.ToString());
            }
        }

        public void SaveTable(double[,] matrix, string[] rows, string[] columns)
        {
            lock (_path)
            {
                string table = FormatSimplexTable(matrix, rows, columns);
                File.AppendAllText(_path, table);
                File.AppendAllText(_path, "\n" + new string('-', 50) + "\n");
            }
        }


        public void ResultSimplexSave(double[] x)
        {
            lock (_path)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("\nЗнайдено опорний розв’язок:");
                sb.Append("X = (");
                sb.Append(string.Join("; ", x.Select(val => val.ToString("F2"))));
                sb.AppendLine(")");
                sb.AppendLine(new string('-', 50));

                File.AppendAllText(_path, sb.ToString());
            }
        }

        public void ResultSimplexSave(double[] x, double zValue)
        {
            lock (_path)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("\nРезультат:");
                sb.Append("X = (");
                sb.Append(string.Join("; ", x.Select(v => v.ToString("F2"))));
                sb.AppendLine(")");
                sb.AppendLine($"Z = {zValue:F2}\n");
                File.AppendAllText(_path, sb.ToString());
            }
        }

        public void SaveSectionHeader(string message)
        {
            lock (_path)
            {
                File.AppendAllText(_path, $"\n{new string('=', 20)}\n{message}\n{new string('=', 20)}\n");
            }
        }
    }
}
