using ConsoleApp1;
using PacmanConsole;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        bool exit = false;

        while (!exit)
        {
            VisualEffects.ShowLoadingScreen();
            VisualEffects.ShowPacmanMenu();
            string option = (Console.ReadLine());

            switch (option)
            {
                case "1":
                    SolveKnapsack();
                    break;

                case "2":
                    PerformSensitivityAnalysisMenu();
                    break;
                case "3":
                    VisualEffects.ShowLoadingScreen();
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("=============================== SIMPLEX SOLVER ===============================");
                    Console.ResetColor();

                    // Objective function
                    Console.WriteLine("\nEnter LP problem in format:");
                    Console.WriteLine("MAX z = 8x1 + 5x2");
                    Console.Write("Objective: ");
                    string objInput = Console.ReadLine();

                    bool isMax = objInput.Trim().ToUpper().StartsWith("MAX");
                    string objClean = objInput.Substring(objInput.IndexOf('=') + 1).Trim();
                    double[] objective = SimplexSolutions.ParseCoefficients(objClean);

                    // Number of constraints
                    Console.Write("\nHow many constraints? ");
                    int m = int.Parse(Console.ReadLine());
                    List<double[]> constraints = new List<double[]>();
                    List<double> rhs = new List<double>();

                    // Constraints input
                    for (int i = 0; i < m; i++)
                    {
                        Console.Write($"Constraint {i + 1} (e.g. 2x1 + x2 <= 10): ");
                        string cons = Console.ReadLine();

                        string[] parts = cons.Split(new string[] { "<=", ">=", "=" }, StringSplitOptions.None);
                        if (parts.Length != 2)
                        {
                            Console.WriteLine("Invalid constraint format. Example: 2x1 + x2 <= 10");
                            return;
                        }

                        double[] coeffs = SimplexSolutions.ParseCoefficients(parts[0]);
                        double b = double.Parse(parts[1], CultureInfo.InvariantCulture);

                        constraints.Add(coeffs);
                        rhs.Add(b);
                    }

                    // Print problem in readable form
                    Console.WriteLine("\n--- Problem Entered ---");
                    Console.WriteLine(objInput);
                    for (int i = 0; i < m; i++)
                        Console.WriteLine($"Constraint {i + 1}: " +
                                          $"{string.Join(" + ", constraints[i])} <= {rhs[i]}");

                    // Build canonical tableau
                    int n = objective.Length;
                    double[,] tableau = new double[m + 1, n + m + 1];

                    // Fill objective row
                    for (int j = 0; j < n; j++)
                        tableau[m, j] = isMax ? -objective[j] : objective[j]; // -c for MAX

                    // Fill constraints rows
                    for (int i = 0; i < m; i++)
                    {
                        for (int j = 0; j < constraints[i].Length; j++)
                            tableau[i, j] = constraints[i][j];
                        tableau[i, n + i] = 1;          // Slack variable
                        tableau[i, n + m] = rhs[i];     // RHS
                    }

                    // Solve using simplex
                    SimplexSolutions.Simplex(tableau, m, n);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n=============================== DONE ===============================");
                    Console.ResetColor();
                    
                    break;
                case "4":
                    exit = true;
                    break;
                case "5":
                    VisualEffects.ShowLoadingScreen();
                    VisualEffects.ShowPacmanMenu();

                    string choice = Console.ReadLine();
                    Console.WriteLine("You chose option: " + choice);
                    break;

                default:
                    Console.WriteLine("Invalid option. Please choose again.");
                    break;
            }
        }
    }

    static void SolveKnapsack()
    {
        Console.WriteLine("Enter the input file path:");
        string inputFilePath = Console.ReadLine();
        Console.WriteLine("Enter the output file path:");
        string outputFilePath = Console.ReadLine();

        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine("Input file not found.");
            return;
        }

        try
        {
            var (capacity, items, objectiveType) = FileHandler.ReadInputFile(inputFilePath);

            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                BranchAndBoundKnapsack solver = new BranchAndBoundKnapsack(capacity, items, writer, objectiveType);
                KnapsackSolution solution = solver.Solve();

                writer.WriteLine();
                writer.WriteLine("Best Solution:");
                writer.WriteLine($"Total Value: {solution.TotalValue}");
                writer.WriteLine($"Total Weight: {solution.TotalWeight}");
                writer.WriteLine($"Selected Items: {string.Join(", ", solution.SelectedItems.Select((s, i) => s == 1 ? $"Item {i + 1}" : string.Empty).Where(x => !string.IsNullOrEmpty(x)))}");

                Console.WriteLine("Solution has been written to the output file.");
                Console.WriteLine($"Total Value: {solution.TotalValue}");
                Console.WriteLine($"Total Weight: {solution.TotalWeight}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }

    static void PerformSensitivityAnalysisMenu()
    {
        Console.WriteLine("Enter the input file path:");
        string inputFilePath = Console.ReadLine();
        Console.WriteLine("Enter the output file path:");
        string outputFilePath = Console.ReadLine();

        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine("Input file not found.");
            return;
        }

        try
        {
            var (capacity, items, objectiveType) = FileHandler.ReadInputFile(inputFilePath);

            using (StreamWriter writer = new StreamWriter(outputFilePath, true))
            {
                BranchAndBoundKnapsack solver = new BranchAndBoundKnapsack(capacity, items, writer, objectiveType);
                KnapsackSolution solution = solver.Solve();

                writer.WriteLine();
                writer.WriteLine("Sensitivity Analysis:");
                var sensitivityResults = new List<string>();

                while (true)
                {
                    Console.WriteLine("Choose an option for Sensitivity Analysis:");
                    Console.WriteLine("1. Display the range of a selected Non-Basic Variable");
                    Console.WriteLine("2. Apply and display a change of a selected Non-Basic Variable");
                    Console.WriteLine("3. Display the range of a selected Basic Variable");
                    Console.WriteLine("4. Apply and display a change of a selected Basic Variable");
                    Console.WriteLine("5. Display the range of a selected constraint RHS value");
                    Console.WriteLine("6. Apply and display a change of a selected constraint RHS value");
                    Console.WriteLine("7. Add a new activity to the model");
                    Console.WriteLine("8. Add a new constraint to the model");
                    Console.WriteLine("9. Display the shadow prices");
                    Console.WriteLine("10. Apply Duality to the programming model");
                    Console.WriteLine("11. Solve the Dual Programming Model");
                    Console.WriteLine("12. Exit Sensitivity Analysis");

                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            Console.WriteLine("Enter the index of the Non-Basic Variable:");
                            int nonBasicIndex = int.Parse(Console.ReadLine()) - 1;
                            sensitivityResults.Add($"Displaying range for Non-Basic Variable x{nonBasicIndex + 1}");
                            SensitivityAnalysis.DisplayRangeAndApplyChange(sensitivityResults, solver, items, nonBasicIndex, solution, capacity);
                            break;
                        case "2":
                            Console.WriteLine("Enter the index of the Non-Basic Variable:");
                            int nonBasicChangeIndex = int.Parse(Console.ReadLine()) - 1;
                            sensitivityResults.Add($"Applying and displaying change for Non-Basic Variable x{nonBasicChangeIndex + 1}");
                            SensitivityAnalysis.DisplayRangeAndApplyChange(sensitivityResults, solver, items, nonBasicChangeIndex, solution, capacity);
                            break;
                        case "3":
                            Console.WriteLine("Enter the index of the Basic Variable:");
                            int basicIndex = int.Parse(Console.ReadLine()) - 1;
                            sensitivityResults.Add($"Displaying range for Basic Variable x{basicIndex + 1}");
                            SensitivityAnalysis.DisplayRangeAndApplyChange(sensitivityResults, solver, items, basicIndex, solution, capacity);
                            break;
                        case "4":
                            Console.WriteLine("Enter the index of the Basic Variable:");
                            int basicChangeIndex = int.Parse(Console.ReadLine()) - 1;
                            sensitivityResults.Add($"Applying and displaying change for Basic Variable x{basicChangeIndex + 1}");
                            SensitivityAnalysis.DisplayRangeAndApplyChange(sensitivityResults, solver, items, basicChangeIndex, solution, capacity);
                            break;
                        case "5":
                            sensitivityResults.Add("Displaying range for constraint RHS values");
                            foreach (var item in items)
                            {
                                sensitivityResults.Add($"Item {items.IndexOf(item) + 1} weight: {item.Weight}");
                            }
                            break;
                        case "6":
                            Console.WriteLine("Enter the index of the constraint RHS value:");
                            int constraintIndex = int.Parse(Console.ReadLine()) - 1;
                            sensitivityResults.Add($"Applying and displaying change for constraint RHS value {constraintIndex + 1}");
                            int originalWeight = items[constraintIndex].Weight;
                            items[constraintIndex].Weight += 1;
                            var newSolution = solver.Solve();
                            sensitivityResults.Add($"After increasing weight of item {constraintIndex + 1} by 1, new Objective Value = {newSolution.TotalValue}");
                            items[constraintIndex].Weight = originalWeight;
                            break;
                        case "7":
                            sensitivityResults.Add("Adding a new activity to the model");
                            items.Add(new KnapsackItem { Weight = 1, Value = 1 });
                            var solutionWithNewActivity = solver.Solve();
                            sensitivityResults.Add($"Objective Value with new activity = {solutionWithNewActivity.TotalValue}");
                            break;
                        case "8":
                            sensitivityResults.Add("Adding a new constraint to the model");
                            items.Last().Weight += 1;  // Example constraint adjustment
                            var solutionWithNewConstraint = solver.Solve();
                            sensitivityResults.Add($"Objective Value with new constraint = {solutionWithNewConstraint.TotalValue}");
                            break;
                        case "9":
                            sensitivityResults.Add("Displaying shadow prices");
                            foreach (var item in items)
                            {
                                sensitivityResults.Add($"Shadow price for item {items.IndexOf(item) + 1}: {SensitivityAnalysis.CalculateShadowPrice(item, solution.TotalValue)}");
                            }
                            break;
                        case "10":
                            sensitivityResults.Add("Applying Duality to the programming model");
                            var dualSolution = SensitivityAnalysis.ApplyDuality(items, out int dualValue);
                            sensitivityResults.Add($"Dual Objective Value: {dualValue}");
                            sensitivityResults.Add(dualValue == solution.TotalValue ? "Strong Duality" : "Weak Duality");
                            break;
                        case "11":
                            sensitivityResults.Add("Solving the Dual Programming Model");
                            // Implement dual programming model solution if needed
                            break;
                        case "12":
                            FileHandler.WriteSensitivityAnalysisResults(outputFilePath, sensitivityResults);
                            return;
                        default:
                            Console.WriteLine("Invalid choice. Please choose again.");
                            break;
                    }

                    Console.WriteLine("Sensitivity Analysis results:");
                    foreach (var result in sensitivityResults)
                    {
                        Console.WriteLine(result);
                    }

                    Console.WriteLine("\nDo you want to continue with Sensitivity Analysis? (yes/no)");
                    string continueAnalysis = Console.ReadLine().ToLower();
                    if (continueAnalysis != "yes")
                    {
                        FileHandler.WriteSensitivityAnalysisResults(outputFilePath, sensitivityResults);
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }
}