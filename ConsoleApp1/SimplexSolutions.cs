using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
  
    // Implements the Simplex algorithm for solving Linear Programming (LP) problems
    // in tableau form.

    public static class SimplexSolutions
    {
        // <summary>
        // Runs the Simplex method on a given tableau.
        // </summary>
         /// <param name="tableau">The simplex tableau (constraints + objective function)</param>
         /// <param name="m">Number of constraints</param>
        /// <param name="n">Number of decision variables</param>
        /// <param name="eps">Numerical tolerance for comparisons</param>
        /// <param name="showIterations">If true, prints each iteration step</param>
        public static void Simplex(double[,] tableau, int m, int n, double eps = 1e-9, bool showIterations = true)
        {
            int rows = tableau.GetLength(0);
            int cols = tableau.GetLength(1);

            // Validate dimensions: rows = constraints + objective, cols = vars + slacks + RHS
            if (rows != m + 1)
                throw new ArgumentException("Tableau row count must be m + 1 (constraints + objective row).");
            if (cols != n + m + 1)
                throw new ArgumentException("Tableau col count must be n + m + 1 (variables + slacks + RHS).");

            // Detect/initialize basis (slack variables at start)
            int[] basis = DetectBasis(tableau, m, eps);
            for (int i = 0; i < m; i++)
                if (basis[i] < 0) basis[i] = n + i; // fallback: assign slack variable as basic

            int iteration = 0;
            if (showIterations) PrintTableau(tableau, m, n, basis, iteration);

            // Iterative process of simplex
            while (true)
            {
                // Choose entering variable (most negative coefficient in objective row)
                int entering = ChooseEntering(tableau, m, cols, eps);
                if (entering == -1) break; // Optimal solution found

                // Choose leaving variable using minimum ratio test
                int leaving = ChooseLeaving(tableau, m, entering, cols, eps);
                if (leaving == -1)
                {
                    Console.WriteLine("Unbounded problem (no valid leaving variable).");
                    return;
                }

                // Perform pivot to update tableau
                Pivot(tableau, leaving, entering, eps);
                basis[leaving] = entering; // update basis

                iteration++;
                if (showIterations) PrintTableau(tableau, m, n, basis, iteration);
            }

            // Final solution output
            Console.WriteLine("\n--- Optimal Solution ---");
            PrintSolution(tableau, m, n, basis, eps);
            Console.WriteLine($"Optimal Value Z = {tableau[m, cols - 1]:F6}");
        }

   
        // Selects entering variable (using Bland’s rule if tie).
        private static int ChooseEntering(double[,] tab, int m, int cols, double eps)
        {
            int entering = -1;
            double mostNegative = -eps;

            for (int j = 0; j < cols - 1; j++) // exclude RHS column
            {
                double rc = tab[m, j]; // reduced cost in objective row
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

        // Selects leaving variable using the minimum ratio test.
        private static int ChooseLeaving(double[,] tab, int m, int entering, int cols, double eps)
        {
            int leaving = -1;
            double bestRatio = double.PositiveInfinity;

            for (int i = 0; i < m; i++)
            {
                double a = tab[i, entering];
                if (a > eps) // valid pivot column entry
                {
                    double ratio = tab[i, cols - 1] / a; // RHS / column value
                    if (ratio < bestRatio - eps || (Math.Abs(ratio - bestRatio) <= eps && (leaving == -1 || i < leaving)))
                    {
                        bestRatio = ratio;
                        leaving = i;
                    }
                }
            }
            return leaving; // -1 => unbounded
        }


        // Performs pivoting on the tableau at (pivotRow, pivotCol).
        public static void Pivot(double[,] tab, int pivotRow, int pivotCol, double eps = 1e-9)
        {
            int rows = tab.GetLength(0);
            int cols = tab.GetLength(1);

            double piv = tab[pivotRow, pivotCol];
            if (Math.Abs(piv) < eps) throw new InvalidOperationException("Pivot is numerically zero.");

            // Normalize pivot row
            for (int j = 0; j < cols; j++)
                tab[pivotRow, j] /= piv;

            // Eliminate pivot column from all other rows
            for (int i = 0; i < rows; i++)
            {
                if (i == pivotRow) continue;
                double factor = tab[i, pivotCol];
                if (Math.Abs(factor) <= eps) continue;

                for (int j = 0; j < cols; j++)
                    tab[i, j] -= factor * tab[pivotRow, j];
            }

            // Clean up very small numerical values
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (Math.Abs(tab[i, j]) < eps) tab[i, j] = 0.0;
        }


        // Attempts to detect initial basis (columns forming identity matrix).
        private static int[] DetectBasis(double[,] tab, int m, double eps)
        {
            int cols = tab.GetLength(1);
            int[] basis = new int[m];
            for (int i = 0; i < basis.Length; i++)
                basis[i] = -1;

            for (int i = 0; i < m; i++)
            {
                int candidate = -1;
                for (int j = 0; j < cols - 1; j++) // skip RHS
                {
                    // Check if column j is approximately a unit vector
                    bool isOneHere = Math.Abs(tab[i, j] - 1.0) <= eps;
                    if (!isOneHere) continue;

                    bool zerosElsewhere = true;
                    for (int r = 0; r < m; r++)
                    {
                        if (r == i) continue;
                        if (Math.Abs(tab[r, j]) > eps) { zerosElsewhere = false; break; }
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


        // Builds column names for variables, slacks, and RHS.
        private static string[] BuildColumnNames(int n, int m)
        {
            var names = new List<string>();
            for (int j = 1; j <= n; j++) names.Add($"x{j}");
            for (int j = 1; j <= m; j++) names.Add($"s{j}");
            names.Add("RHS");
            return names.ToArray();
        }

 
        /// Returns a variable's name given its index.
        private static string VarName(int idx, int n, int m)
        {
            if (idx < n) return $"x{idx + 1}";
            if (idx < n + m) return $"s{idx - n + 1}";
            return $"v{idx + 1}";
        }


        // Prints the current tableau with basis info.
        private static void PrintTableau(double[,] tab, int m, int n, int[] basis, int iteration)
        {
            int cols = tab.GetLength(1);
            string[] headers = BuildColumnNames(n, m);

            Console.WriteLine($"\n===== Iteration {iteration} =====");
            Console.Write($"{"Basis",7} |");
            for (int j = 0; j < headers.Length; j++)
                Console.Write($"{headers[j],10}");
            Console.WriteLine();
            Console.WriteLine(new string('-', 8 + 10 * headers.Length));

            // Print constraint rows
            for (int i = 0; i < m; i++)
            {
                string rowHead = basis[i] >= 0 ? VarName(basis[i], n, m) : "—";
                Console.Write($"{rowHead,7} |");
                for (int j = 0; j < cols; j++)
                    Console.Write($"{tab[i, j],10:F4}");
                Console.WriteLine();
            }

            // Print objective row
            Console.Write($"{"Z",7} |");
            for (int j = 0; j < cols; j++)
                Console.Write($"{tab[m, j],10:F4}");
            Console.WriteLine();
        }

  
        // Prints the final solution (values of x and slack variables).
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

            // Print decision variables
            for (int j = 0; j < n; j++)
                Console.WriteLine($"x{j + 1} = {x[j]:F6}");
            // Print slack variables
            for (int j = 0; j < m; j++)
                Console.WriteLine($"s{j + 1} = {x[n + j]:F6}");
        }

    
        // Parses a linear expression like "3x1 + 2x2 - x3" into coefficients.
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
