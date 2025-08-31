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
        bool exit = false; // Flag to control program loop

        // Main loop: keeps running until user chooses "Exit"
        while (!exit)
        {
            Console.Clear(); // Clear console each time menu reloads
            VisualEffects.ShowLoadingScreen(); // Show a custom loading animation
            VisualEffects.ShowPacmanMenu();    // Show Pacman-style menu
            string option = (Console.ReadLine()); // Read user choice

            // Handle user input from menu
            switch (option)
            {
                case "1":
                    // Knapsack Problem solver
                    VisualEffects.ShowLoadingScreen();
                    SolveKnapsack();
                    break;

                case "2":
                    // Sensitivity Analysis Menu
                    VisualEffects.ShowLoadingScreen();
                    PerformSensitivityAnalysisMenu();
                    break;

                case "3":
                    // Simplex Solver for Linear Programming
                    VisualEffects.ShowLoadingScreen();
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("=============================== SIMPLEX SOLVER ===============================");
                    Console.ResetColor();

                    // --- Objective Function Input ---
                    Console.WriteLine("\nEnter LP problem in format:");
                    Console.WriteLine("MAX z = 8x1 + 5x2");
                    Console.Write("Objective: ");
                    string objInput = Console.ReadLine();

                    // Detect if maximization or minimization
                    bool isMax = objInput.Trim().ToUpper().StartsWith("MAX");
                    string objClean = objInput.Substring(objInput.IndexOf('=') + 1).Trim();

                    // Parse coefficients of objective function
                    double[] objective = SimplexSolutions.ParseCoefficients(objClean);

                    // --- Constraint Input ---
                    Console.Write("\nHow many constraints? ");
                    int m = int.Parse(Console.ReadLine());
                    List<double[]> constraints = new List<double[]>();
                    List<double> rhs = new List<double>();

                    // Read each constraint
                    for (int i = 0; i < m; i++)
                    {
                        Console.Write($"Constraint {i + 1} (e.g. 2x1 + x2 <= 10): ");
                        string cons = Console.ReadLine();

                        // Split constraint into LHS and RHS
                        string[] parts = cons.Split(new string[] { "<=", ">=", "=" }, StringSplitOptions.None);
                        if (parts.Length != 2)
                        {
                            Console.WriteLine("Invalid constraint format. Example: 2x1 + x2 <= 10");
                            return;
                        }

                        // Parse LHS coefficients
                        double[] coeffs = SimplexSolutions.ParseCoefficients(parts[0]);

                        // Parse RHS value
                        double b = double.Parse(parts[1], CultureInfo.InvariantCulture);

                        constraints.Add(coeffs);
                        rhs.Add(b);
                    }

                    // --- Display Entered Problem ---
                    Console.WriteLine("\n--- Problem Entered ---");
                    Console.WriteLine(objInput);
                    for (int i = 0; i < m; i++)
                        Console.WriteLine($"Constraint {i + 1}: " +
                                          $"{string.Join(" + ", constraints[i])} <= {rhs[i]}");

                    // --- Build Simplex Tableau ---
                    int n = objective.Length;
                    double[,] tableau = new double[m + 1, n + m + 1];

                    // Fill objective row (negative for MAX problems)
                    for (int j = 0; j < n; j++)
                        tableau[m, j] = isMax ? -objective[j] : objective[j];

                    // Fill constraints rows
                    for (int i = 0; i < m; i++)
                    {
                        for (int j = 0; j < constraints[i].Length; j++)
                            tableau[i, j] = constraints[i][j];
                        tableau[i, n + i] = 1;          // Add slack variable
                        tableau[i, n + m] = rhs[i];     // RHS values
                    }

                    // --- Solve LP with Simplex ---
                    SimplexSolutions.Simplex(tableau, m, n);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n=============================== DONE ===============================");
                    Console.ReadKey();
                    Console.ResetColor();

                    break;

                case "4":
                    // Exit program
                    exit = true;
                    break;

                case "5":
                    // (Extra option, possibly for debugging/testing menu reloads)
                    VisualEffects.ShowLoadingScreen();
                    VisualEffects.ShowPacmanMenu();

                    string choice = Console.ReadLine();
                    Console.WriteLine("You chose option: " + choice);
                    break;

                default:
                    // Invalid menu choice
                    Console.WriteLine("Invalid option. Please choose again.");
                    break;
            }
        }
    }

    // --- Method to Solve Knapsack Problem ---
    static void SolveKnapsack()
    {
        Console.WriteLine("Enter the input file path:");
        string inputFilePath = Console.ReadLine();
        Console.WriteLine("Enter the output file path:");
        string outputFilePath = Console.ReadLine();

        // Check if input file exists
        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine("Input file not found.");
            return;
        }

        try
        {
            // Read knapsack problem instance
            var (capacity, items, objectiveType) = FileHandler.ReadInputFile(inputFilePath);

            // Solve using Branch and Bound Knapsack solver
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                BranchAndBoundKnapsack solver = new BranchAndBoundKnapsack(capacity, items, writer, objectiveType);
                KnapsackSolution solution = solver.Solve();

                // Write solution to output file
                writer.WriteLine();
                writer.WriteLine("Best Solution:");
                writer.WriteLine($"Total Value: {solution.TotalValue}");
                writer.WriteLine($"Total Weight: {solution.TotalWeight}");
                writer.WriteLine($"Selected Items: {string.Join(", ", solution.SelectedItems.Select((s, i) => s == 1 ? $"Item {i + 1}" : string.Empty).Where(x => !string.IsNullOrEmpty(x)))}");

                // Display solution in console
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

    // --- Method to Perform Sensitivity Analysis ---
    static void PerformSensitivityAnalysisMenu()
    {
        Console.WriteLine("Enter the input file path:");
        string inputFilePath = Console.ReadLine();
        Console.WriteLine("Enter the output file path:");
        string outputFilePath = Console.ReadLine();

        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine("Input file not found.");
            Console.ReadKey();
            return;
        }

        try
        {
            // Read problem instance
            var (capacity, items, objectiveType) = FileHandler.ReadInputFile(inputFilePath);

            using (StreamWriter writer = new StreamWriter(outputFilePath, true))
            {
                BranchAndBoundKnapsack solver = new BranchAndBoundKnapsack(capacity, items, writer, objectiveType);
                KnapsackSolution solution = solver.Solve();

                writer.WriteLine();
                writer.WriteLine("Sensitivity Analysis:");
                var sensitivityResults = new List<string>();

                // Sensitivity analysis interactive menu
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

                    // Handle user choice
                    switch (choice)
                    {
                        case "1":
                            // Range of Non-Basic Variable
                            Console.WriteLine("Enter the index of the Non-Basic Variable:");
                            int nonBasicIndex = int.Parse(Console.ReadLine()) - 1;
                            sensitivityResults.Add($"Displaying range for Non-Basic Variable x{nonBasicIndex + 1}");
                            SensitivityAnalysis.DisplayRangeAndApplyChange(sensitivityResults, solver, items, nonBasicIndex, solution, capacity);
                            break;

                        case "2":
                            // Apply change to Non-Basic Variable
                            Console.WriteLine("Enter the index of the Non-Basic Variable:");
                            int nonBasicChangeIndex = int.Parse(Console.ReadLine()) - 1;
                            sensitivityResults.Add($"Applying and displaying change for Non-Basic Variable x{nonBasicChangeIndex + 1}");
                            SensitivityAnalysis.DisplayRangeAndApplyChange(sensitivityResults, solver, items, nonBasicChangeIndex, solution, capacity);
                            break;

                        case "3":
                            // Range of Basic Variable
                            Console.WriteLine("Enter the index of the Basic Variable:");
                            int basicIndex = int.Parse(Console.ReadLine()) - 1;
                            sensitivityResults.Add($"Displaying range for Basic Variable x{basicIndex + 1}");
                            SensitivityAnalysis.DisplayRangeAndApplyChange(sensitivityResults, solver, items, basicIndex, solution, capacity);
                            break;

                        case "4":
                            // Apply change to Basic Variable
                            Console.WriteLine("Enter the index of the Basic Variable:");
                            int basicChangeIndex = int.Parse(Console.ReadLine()) - 1;
                            sensitivityResults.Add($"Applying and displaying change for Basic Variable x{basicChangeIndex + 1}");
                            SensitivityAnalysis.DisplayRangeAndApplyChange(sensitivityResults, solver, items, basicChangeIndex, solution, capacity);
                            break;

                        case "5":
                            // Display RHS ranges
                            sensitivityResults.Add("Displaying range for constraint RHS values");
                            foreach (var item in items)
                            {
                                sensitivityResults.Add($"Item {items.IndexOf(item) + 1} weight: {item.Weight}");
                            }
                            break;

                        case "6":
                            // Apply change to RHS value
                            Console.WriteLine("Enter the index of the constraint RHS value:");
                            int constraintIndex = int.Parse(Console.ReadLine()) - 1;
                            sensitivityResults.Add($"Applying and displaying change for constraint RHS value {constraintIndex + 1}");
                            int originalWeight = items[constraintIndex].Weight;
                            items[constraintIndex].Weight += 1;
                            var newSolution = solver.Solve();
                            sensitivityResults.Add($"After increasing weight of item {constraintIndex + 1} by 1, new Objective Value = {newSolution.TotalValue}");
                            items[constraintIndex].Weight = originalWeight; // Restore
                            break;

                        case "7":
                            // Add new activity (item)
                            sensitivityResults.Add("Adding a new activity to the model");
                            items.Add(new KnapsackItem { Weight = 1, Value = 1 });
                            var solutionWithNewActivity = solver.Solve();
                            sensitivityResults.Add($"Objective Value with new activity = {solutionWithNewActivity.TotalValue}");
                            break;

                        case "8":
                            // Add new constraint (adjust item weight as example)
                            sensitivityResults.Add("Adding a new constraint to the model");
                            items.Last().Weight += 1;
                            var solutionWithNewConstraint = solver.Solve();
                            sensitivityResults.Add($"Objective Value with new constraint = {solutionWithNewConstraint.TotalValue}");
                            break;

                        case "9":
                            // Display shadow prices
                            sensitivityResults.Add("Displaying shadow prices");
                            foreach (var item in items)
                            {
                                sensitivityResults.Add($"Shadow price for item {items.IndexOf(item) + 1}: {SensitivityAnalysis.CalculateShadowPrice(item, solution.TotalValue)}");
                            }
                            break;

                        case "10":
                            // Apply duality
                            sensitivityResults.Add("Applying Duality to the programming model");
                            var dualSolution = SensitivityAnalysis.ApplyDuality(items, out int dualValue);
                            sensitivityResults.Add($"Dual Objective Value: {dualValue}");
                            sensitivityResults.Add(dualValue == solution.TotalValue ? "Strong Duality" : "Weak Duality");
                            break;

                        case "11":
                            // Solve dual model
                            sensitivityResults.Add("Solving the Dual Programming Model");
                            // Dual model logic would be added here
                            break;

                        case "12":
                            // Exit and save sensitivity results
                            FileHandler.WriteSensitivityAnalysisResults(outputFilePath, sensitivityResults);
                            return;

                        default:
                            Console.WriteLine("Invalid choice. Please choose again.");
                            break;
                    }

                    // Show results so far
                    Console.WriteLine("Sensitivity Analysis results:");
                    foreach (var result in sensitivityResults)
                    {
                        Console.WriteLine(result);
                    }

                    // Ask user if they want to continue
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
