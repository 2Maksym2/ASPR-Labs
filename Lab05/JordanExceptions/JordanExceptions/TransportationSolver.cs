using System.Globalization;
using System.Text;

namespace JordanExceptions;

public static class TransportationSolver
{
    private const double Eps = 1e-9;

    public enum InitialMethod
    {
        NorthWestCorner,
        MinimumCost
    }

    public class Result
    {
        public double[,] InitialPlan { get; init; } = null!;
        public double InitialCost { get; init; }
        public double[,] OptimalPlan { get; init; } = null!;
        public double OptimalCost { get; init; }
        public string Protocol { get; init; } = "";
    }

    public static Result Solve(double[,] costs, double[] supply, double[] demand, InitialMethod initialMethod)
    {
        int n = supply.Length;
        int m = demand.Length;
        if (costs.GetLength(0) != n || costs.GetLength(1) != m)
            throw new ArgumentException("Розмірність матриці вартостей не збігається з запасами та заявками.");

        double sumS = supply.Sum();
        double sumD = demand.Sum();
        if (Math.Abs(sumS - sumD) > 1e-6 * Math.Max(1, Math.Max(sumS, sumD)))
            throw new ArgumentException("Задача не збалансована: сума запасів має дорівнювати сумі заявок.");

        var sb = new StringBuilder();
        AppendInputSection(sb, costs, supply, demand, n, m);

        double[] s = (double[])supply.Clone();
        double[] d = (double[])demand.Clone();

        double[,] initial;
        List<(int i, int j, double val)> fillSteps;
        if (initialMethod == InitialMethod.NorthWestCorner)
        {
            initial = NorthWestCornerWithSteps(s, d, n, m, out fillSteps);
            sb.AppendLine("Пошук опорного плану перевезень методом північно-західного кута:");
            AppendFillSequence(sb, fillSteps);
        }
        else
        {
            initial = MinimumCostWithSteps((double[,])costs.Clone(), s, d, n, m, out fillSteps);
            sb.AppendLine("Пошук опорного плану перевезень методом мінімального елемента:");
            AppendFillSequence(sb, fillSteps);
        }

        sb.AppendLine("Опорний план перевезень:");
        sb.AppendLine(FormatPlanGrid(initial, n, m));
        sb.AppendLine();
        sb.AppendLine("Вартість перевезень за опорним планом:");
        double initialCost = TotalCost(initial, costs, n, m);
        sb.AppendLine(FormatCostFormula(initial, costs, n, m, initialCost));
        sb.AppendLine();

        sb.AppendLine("Пошук оптимального плану перевезень методом потенціалів:");
        double[,] work = (double[,])initial.Clone();
        OptimizePotentialsWithProtocol(work, costs, n, m, sb);
        double optimalCost = TotalCost(work, costs, n, m);

        return new Result
        {
            InitialPlan = initial,
            InitialCost = initialCost,
            OptimalPlan = work,
            OptimalCost = optimalCost,
            Protocol = sb.ToString()
        };
    }

    private static void AppendInputSection(StringBuilder sb, double[,] costs, double[] supply, double[] demand, int n, int m)
    {
        sb.AppendLine("Матриця вартостей:");
        sb.AppendLine(FormatIntMatrix(costs, n, m));
        sb.AppendLine();
        sb.AppendLine("Вектор запасів:");
        sb.AppendLine(string.Join("  ", supply.Select(FmtNum)));
        sb.AppendLine();
        sb.AppendLine("Вектор заявок:");
        sb.AppendLine(string.Join("  ", demand.Select(FmtNum)));
        sb.AppendLine();
    }

    private static void AppendFillSequence(StringBuilder sb, List<(int i, int j, double val)> steps)
    {
        sb.Append("Послідовність заповнення таблиці: ");
        for (int k = 0; k < steps.Count; k++)
        {
            var (i, j, v) = steps[k];
            sb.Append($"(x{i + 1}{j + 1} = {FmtNum(v)})");
            if (k < steps.Count - 1) sb.Append("->");
        }
        sb.AppendLine();
        sb.AppendLine();
    }

    private static string FormatIntMatrix(double[,] a, int n, int m)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (j > 0) sb.Append("  ");
                sb.Append(FmtNum(a[i, j]));
            }
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    private static string FormatPlanGrid(double[,] x, int n, int m)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (j > 0) sb.Append("  ");
                if (x[i, j] > Eps)
                    sb.Append(FmtNum(x[i, j]));
                else
                    sb.Append('x');
            }
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    private static string FormatCostFormula(double[,] x, double[,] c, int n, int m, double total)
    {
        var terms = new List<string>();
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (x[i, j] <= Eps) continue;
                terms.Add($"{FmtNum(x[i, j])} * {FmtNum(c[i, j])}");
            }
        }

        return $"S = {string.Join(" + ", terms)} = {FmtNum(total)}";
    }

    private static string FmtNum(double v) =>
        Math.Abs(v - Math.Round(v, 6)) < 1e-5
            ? ((int)Math.Round(v)).ToString(CultureInfo.InvariantCulture)
            : v.ToString("0.##", CultureInfo.InvariantCulture);

    private static double TotalCost(double[,] x, double[,] c, int n, int m)
    {
        double t = 0;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                t += x[i, j] * c[i, j];
        return t;
    }

    private static double[,] NorthWestCornerWithSteps(double[] supply, double[] demand, int n, int m, out List<(int i, int j, double val)> steps)
    {
        steps = new List<(int i, int j, double val)>();
        double[,] x = new double[n, m];
        int i = 0, j = 0;
        while (true)
        {
            if (i >= n || j >= m) break;

            double v = Math.Min(supply[i], demand[j]);
            x[i, j] = v;
            supply[i] -= v;
            demand[j] -= v;
            steps.Add((i, j, v));

            if (supply[i] > Eps)
            {
                j++;
                if (j >= m) break;
            }
            else if (demand[j] > Eps)
            {
                i++;
                if (i >= n) break;
            }
            else
            {
                i++;
                j++;
                if (j >= m && i >= n) break;
            }
        }

        return x;
    }

    private static double[,] MinimumCostWithSteps(double[,] costs, double[] supply, double[] demand, int n, int m, out List<(int i, int j, double val)> steps)
    {
        steps = new List<(int i, int j, double val)>();
        double[,] x = new double[n, m];
        var activeRow = Enumerable.Repeat(true, n).ToArray();
        var activeCol = Enumerable.Repeat(true, m).ToArray();

        while (activeRow.Any(r => r) && activeCol.Any(c => c))
        {
            double best = double.PositiveInfinity;
            int bi = -1, bj = -1;
            for (int ii = 0; ii < n; ii++)
            {
                if (!activeRow[ii] || supply[ii] <= Eps) continue;
                for (int jj = 0; jj < m; jj++)
                {
                    if (!activeCol[jj] || demand[jj] <= Eps) continue;
                    if (costs[ii, jj] < best - Eps)
                    {
                        best = costs[ii, jj];
                        bi = ii;
                        bj = jj;
                    }
                }
            }

            if (bi < 0) break;

            double amt = Math.Min(supply[bi], demand[bj]);
            x[bi, bj] += amt;
            supply[bi] -= amt;
            demand[bj] -= amt;
            steps.Add((bi, bj, amt));

            if (supply[bi] <= Eps) activeRow[bi] = false;
            if (demand[bj] <= Eps) activeCol[bj] = false;
        }

        return x;
    }

    private static bool IsBasic(double[,] x, int i, int j) => x[i, j] > Eps;

    private static void OptimizePotentialsWithProtocol(double[,] x, double[,] c, int n, int m, StringBuilder sb)
    {
        const int maxIter = 5000;
        for (int iter = 0; iter < maxIter; iter++)
        {
            var u = new double[n];
            var v = new double[m];
            if (!TryComputePotentials(x, c, n, m, u, v))
            {
                sb.AppendLine("Не вдалося обчислити потенціали (можлива виродженість опорного плану).");
                break;
            }

            sb.AppendLine("Потенціали постачальників:");
            sb.AppendLine(string.Join("  ", u.Select(FmtNum)));
            sb.AppendLine("Потенціали споживачів:");
            sb.AppendLine(string.Join("  ", v.Select(FmtNum)));
            sb.AppendLine();
            sb.AppendLine("Непрямі вартості:");
            sb.AppendLine(FormatIndirectGrid(u, v, x, n, m));
            sb.AppendLine();

            var problematic = new List<(int i, int j)>();
            double bestExcess = 0;
            int ei = -1, ej = -1;
            for (int ii = 0; ii < n; ii++)
            {
                for (int jj = 0; jj < m; jj++)
                {
                    if (IsBasic(x, ii, jj)) continue;
                    double indirect = u[ii] + v[jj];
                    double excess = indirect - c[ii, jj];
                    if (excess > 1e-9)
                    {
                        problematic.Add((ii, jj));
                        if (excess > bestExcess + 1e-9)
                        {
                            bestExcess = excess;
                            ei = ii;
                            ej = jj;
                        }
                    }
                }
            }

            problematic.Sort((a, b) => a.i != b.i ? a.i.CompareTo(b.i) : a.j.CompareTo(b.j));

            if (ei < 0 || bestExcess <= 1e-9)
            {
                sb.AppendLine("Умова оптимальності виконується.");
                sb.AppendLine();
                sb.AppendLine("Знайдено оптимальний план перевезень:");
                sb.AppendLine(FormatPlanGrid(x, n, m));
                sb.AppendLine();
                sb.AppendLine("Вартість перевезень за оптимальним планом:");
                sb.AppendLine(FormatCostFormula(x, c, n, m, TotalCost(x, c, n, m)));
                break;
            }

            sb.AppendLine("Умова оптимальності не виконується.");
            if (problematic.Count > 0)
            {
                sb.Append("Знайдено «проблемні» клітини: ");
                sb.AppendLine(string.Join("; ", problematic.Select(p => $"[{p.i + 1}, {p.j + 1}]")));
            }
            sb.AppendLine();

            var cycle = FindCycle(ei, ej, x, n, m);
            if (cycle == null || cycle.Count < 4)
            {
                sb.AppendLine("Не вдалося побудувати цикл для покращення плану.");
                break;
            }

            sb.AppendLine("Побудовано цикл:");
            sb.AppendLine(FormatCycleGrid(cycle, n, m));
            sb.AppendLine();

            double costBefore = TotalCost(x, c, n, m);

            double theta = double.PositiveInfinity;
            for (int k = 1; k < cycle.Count; k += 2)
            {
                var (ri, rj) = cycle[k];
                theta = Math.Min(theta, x[ri, rj]);
            }

            if (theta <= Eps || double.IsInfinity(theta))
            {
                sb.AppendLine("Неможливо визначити крок покращення (θ = 0).");
                break;
            }

            for (int k = 0; k < cycle.Count; k++)
            {
                var (ri, rj) = cycle[k];
                double sign = (k % 2 == 0) ? 1 : -1;
                x[ri, rj] += sign * theta;
            }

            for (int ii = 0; ii < n; ii++)
                for (int jj = 0; jj < m; jj++)
                    if (x[ii, jj] < Eps) x[ii, jj] = 0;

            double costAfter = TotalCost(x, c, n, m);
            double economy = costBefore - costAfter;

            sb.AppendLine($"Знайдено значення λ: {FmtNum(theta)}, економія: {FmtNum(economy)}");
            sb.AppendLine();
            sb.AppendLine("Новий план перевезень:");
            sb.AppendLine(FormatPlanGrid(x, n, m));
            sb.AppendLine();
            sb.AppendLine("Вартість перевезень за новим планом:");
            sb.AppendLine(FormatCostFormula(x, c, n, m, costAfter));
            sb.AppendLine();
        }
    }

    private static string FormatIndirectGrid(double[] u, double[] v, double[,] x, int n, int m)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (j > 0) sb.Append("  ");
                if (IsBasic(x, i, j))
                    sb.Append('x');
                else
                {
                    double d = u[i] + v[j];
                    sb.Append(FmtNum(d));
                }
            }
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    private static string FormatCycleGrid(List<(int i, int j)> cycle, int n, int m)
    {
        var marks = new string[n, m];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                marks[i, j] = "x";

        for (int k = 0; k < cycle.Count; k++)
        {
            var (ci, cj) = cycle[k];
            if (k == 0)
                marks[ci, cj] = "λ";
            else
                marks[ci, cj] = (k % 2 == 1) ? "-" : "+";
        }

        var sb = new StringBuilder();
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (j > 0) sb.Append("  ");
                sb.Append(marks[i, j]);
            }
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    private static bool TryComputePotentials(double[,] x, double[,] c, int n, int m, double[] u, double[] v)
    {
        for (int i = 0; i < n; i++) u[i] = double.NaN;
        for (int j = 0; j < m; j++) v[j] = double.NaN;

        int i0 = -1;
        for (int i = 0; i < n && i0 < 0; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (!IsBasic(x, i, j)) continue;
                i0 = i;
                break;
            }
        }

        if (i0 < 0) return false;
        u[i0] = 0;

        bool changed = true;
        int guard = 0;
        while (changed && guard++ < (n + m) * (n + m))
        {
            changed = false;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (!IsBasic(x, i, j)) continue;
                    if (!double.IsNaN(u[i]) && double.IsNaN(v[j]))
                    {
                        v[j] = c[i, j] - u[i];
                        changed = true;
                    }
                    else if (!double.IsNaN(v[j]) && double.IsNaN(u[i]))
                    {
                        u[i] = c[i, j] - v[j];
                        changed = true;
                    }
                }
            }
        }

        for (int i = 0; i < n; i++)
            if (double.IsNaN(u[i])) return false;
        for (int j = 0; j < m; j++)
            if (double.IsNaN(v[j])) return false;
        return true;
    }

    private static List<(int i, int j)>? FindCycle(int si, int sj, double[,] x, int n, int m)
    {
        var basic = new bool[n, m];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                basic[i, j] = IsBasic(x, i, j);
        basic[si, sj] = true;

        var chain = new List<(int i, int j)> { (si, sj) };
        if (SearchFrom(si, sj, true, chain, basic, n, m, si, sj))
        {
            if (chain.Count > 0 && chain[0] == chain[^1])
                chain.RemoveAt(chain.Count - 1);
            return chain;
        }

        return null;
    }

    private static bool SearchFrom(int i, int j, bool searchRow, List<(int i, int j)> chain, bool[,] basic, int n, int m, int si, int sj)
    {
        if (searchRow)
        {
            for (int jj = 0; jj < m; jj++)
            {
                if (jj == j) continue;
                if (!basic[i, jj]) continue;

                if (i == si && jj == sj && chain.Count >= 3)
                    return true;

                chain.Add((i, jj));
                if (SearchFrom(i, jj, false, chain, basic, n, m, si, sj))
                    return true;
                chain.RemoveAt(chain.Count - 1);
            }
        }
        else
        {
            for (int ii = 0; ii < n; ii++)
            {
                if (ii == i) continue;
                if (!basic[ii, j]) continue;

                if (ii == si && j == sj && chain.Count >= 3)
                    return true;

                chain.Add((ii, j));
                if (SearchFrom(ii, j, true, chain, basic, n, m, si, sj))
                    return true;
                chain.RemoveAt(chain.Count - 1);
            }
        }

        return false;
    }
}
