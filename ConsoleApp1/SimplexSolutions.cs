using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    public static class SimplexSolutions
    {
        public static void Simplex(double[,] tableau, int m, int n, double eps = 1e-9, bool showIterations = true)
        {
            int rows = tableau.GetLength(0);
            int cols = tableau.GetLength(1);

            if (rows != m + 1)
                throw new ArgumentException("Tableau row count must be m + 1 (constraints + objective row).");
            if (cols != n + m + 1)
                throw new ArgumentException("Tableau col count must be n + m + 1 (variables + slacks + RHS).");

            // Detect/initialize basis (slacks) and print Iteration 0
            int[] basis = DetectBasis(tableau, m, eps);
            for (int i = 0; i < m; i++)
                if (basis[i] < 0) basis[i] = n + i; // fallback to slacks if identity is exact (standard start)

            int iteration = 0;
            if (showIterations) PrintTableau(tableau, m, n, basis, iteration);

            // Iterate
            while (true)
            {
                int entering = ChooseEntering(tableau, m, cols, eps); // most negative in last row (Bland if tie)
                if (entering == -1) break; // optimal

                int leaving = ChooseLeaving(tableau, m, entering, cols, eps); // min positive ratio
                if (leaving == -1)
                {
                    Console.WriteLine("Unbounded problem (no valid leaving variable).");
                    return;
                }

                Pivot(tableau, leaving, entering, eps);
                basis[leaving] = entering;

                iteration++;
                if (showIterations) PrintTableau(tableau, m, n, basis, iteration);
            }

            // Final solution
            Console.WriteLine("\n--- Optimal Solution ---");
            PrintSolution(tableau, m, n, basis, eps);
            Console.WriteLine($"Optimal Value Z = {tableau[m, cols - 1]:F6}");
        }

        // Bland’s rule on ties; pick smallest index with negative reduced cost in the last row.
        private static int ChooseEntering(double[,] tab, int m, int cols, double eps)
        {
            int entering = -1;
            double mostNegative = -eps; // must be strictly below -eps to improve

            for (int j = 0; j < cols - 1; j++) // exclude RHS
            {
                double rc = tab[m, j];
                if (rc < mostNegative || (Math.Abs(rc - mostNegative) <= eps && entering != -1 && j < entering))
                {
                    if (rc < -eps)
                    {
                        mostNegative = rc;
                        entering = j;
                    }
                }
            }
            return entering; // -1 => optimal
        }

        // Standard min ratio test with epsilon guard; Bland tie-break (smallest row index)
        private static int ChooseLeaving(double[,] tab, int m, int entering, int cols, double eps)
        {
            int leaving = -1;
            double bestRatio = double.PositiveInfinity;

            for (int i = 0; i < m; i++)
            {
                double a = tab[i, entering];
                if (a > eps) // strictly positive pivot column entry
                {
                    double ratio = tab[i, cols - 1] / a;
                    if (ratio < bestRatio - eps || (Math.Abs(ratio - bestRatio) <= eps && (leaving == -1 || i < leaving)))
                    {
                        bestRatio = ratio;
                        leaving = i;
                    }
                }
            }
            return leaving; // -1 => unbounded
        }

        public static void Pivot(double[,] tab, int pivotRow, int pivotCol, double eps = 1e-9)
        {
            int rows = tab.GetLength(0);
            int cols = tab.GetLength(1);

            double piv = tab[pivotRow, pivotCol];
            if (Math.Abs(piv) < eps) throw new InvalidOperationException("Pivot is numerically zero.");

            // Normalize pivot row
            for (int j = 0; j < cols; j++)
                tab[pivotRow, j] /= piv;

            // Eliminate pivot column in other rows
            for (int i = 0; i < rows; i++)
            {
                if (i == pivotRow) continue;
                double factor = tab[i, pivotCol];
                if (Math.Abs(factor) <= eps) continue;

                for (int j = 0; j < cols; j++)
                    tab[i, j] -= factor * tab[pivotRow, j];
            }

            // Clean tiny noise
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (Math.Abs(tab[i, j]) < eps) tab[i, j] = 0.0;
        }

        // Try to detect basic column in each constraint row (≈ identity column)
        private static int[] DetectBasis(double[,] tab, int m, double eps)
        {
            int cols = tab.GetLength(1);
            int[] basis = new int[m];
            for (int i = 0; i < basis.Length; i++)
                basis[i] = -1;

            for (int i = 0; i < m; i++)
            {
                int candidate = -1;
                for (int j = 0; j < cols - 1; j++) // exclude RHS
                {
                    // Check if column j is (approx) unit vector with 1 at row i and 0 elsewhere (in constraint rows only)
                    bool isOneHere = Math.Abs(tab[i, j] - 1.0) <= 1e-9;
                    if (!isOneHere) continue;

                    bool zerosElsewhere = true;
                    for (int r = 0; r < m; r++)
                    {
                        if (r == i) continue;
                        if (Math.Abs(tab[r, j]) > 1e-9) { zerosElsewhere = false; break; }
                    }
                    if (zerosElsewhere)
                    {
                        candidate = j;
                        break;
                    }
                }
                basis[i] = candidate;
            }
            return basis;
        }

        private static string[] BuildColumnNames(int n, int m)
        {
            var names = new List<string>();
            for (int j = 1; j <= n; j++) names.Add($"x{j}");
            for (int j = 1; j <= m; j++) names.Add($"s{j}");
            names.Add("RHS");
            return names.ToArray();
        }

        private static string VarName(int idx, int n, int m)
        {
            if (idx < n) return $"x{idx + 1}";
            if (idx < n + m) return $"s{idx - n + 1}";
            return $"v{idx + 1}";
        }

        private static void PrintTableau(double[,] tab, int m, int n, int[] basis, int iteration)
        {
            int cols = tab.GetLength(1);
            string[] headers = BuildColumnNames(n, m);

            Console.WriteLine($"\n===== Iteration {iteration} =====");
            // Header
            Console.Write($"{"Basis",7} |");
            for (int j = 0; j < headers.Length; j++)
                Console.Write($"{headers[j],10}");
            Console.WriteLine();
            Console.WriteLine(new string('-', 8 + 10 * headers.Length));

            // Constraint rows
            for (int i = 0; i < m; i++)
            {
                string rowHead = basis[i] >= 0 ? VarName(basis[i], n, m) : "—";
                Console.Write($"{rowHead,7} |");
                for (int j = 0; j < cols; j++)
                    Console.Write($"{tab[i, j],10:F4}");
                Console.WriteLine();
            }

            // Objective row
            Console.Write($"{"Z",7} |");
            for (int j = 0; j < cols; j++)
                Console.Write($"{tab[m, j],10:F4}");
            Console.WriteLine();
        }

        private static void PrintSolution(double[,] tab, int m, int n, int[] basis, double eps)
        {
            int cols = tab.GetLength(1);
            double[] x = new double[n + m]; // decision + slacks

            for (int i = 0; i < m; i++)
            {
                int varIdx = basis[i];
                if (varIdx >= 0 && varIdx < n + m)
                    x[varIdx] = tab[i, cols - 1];
            }

            for (int j = 0; j < n; j++)
                Console.WriteLine($"x{j + 1} = {x[j]:F6}");
            for (int j = 0; j < m; j++)
                Console.WriteLine($"s{j + 1} = {x[n + j]:F6}");
        }

        // (Optional) helpers retained from your version
        public static double[] ParseCoefficients(string expr)
        {
            string[] terms = expr.Replace("-", "+-").Split('+');
            List<double> coeffs = new List<double>();
            foreach (string term in terms)
            {
                if (string.IsNullOrWhiteSpace(term)) continue;
                string t = term.Trim();
                if (t.Contains("x"))
                {
                    string[] parts = t.Split('x');
                    double coeff = parts[0] == "" || parts[0] == "+" ? 1 : (parts[0] == "-" ? -1 : double.Parse(parts[0], CultureInfo.InvariantCulture));
                    int index = int.Parse(parts[1], CultureInfo.InvariantCulture);
                    while (coeffs.Count < index) coeffs.Add(0);
                    coeffs[index - 1] = coeff;
                }
            }
            return coeffs.ToArray();
        }

    }
}
