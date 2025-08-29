using System;
using System.Collections.Generic;

/// <summary>
/// Performs sensitivity analysis on the knapsack solution
/// </summary>
public static class SensitivityAnalysis
{
    /// <summary>
    /// Performs comprehensive sensitivity analysis on the knapsack solution
    /// </summary>
    public static List<string> PerformSensitivityAnalysis(BranchAndBoundKnapsack solver,
        List<KnapsackItem> items, KnapsackSolution solution, int capacity)
    {
        var results = new List<string>();

        AnalyzeNonBasicVariables(results, solver, items, solution, capacity);
        AnalyzeBasicVariables(results, solver, items, solution, capacity);
        AnalyzeConstraintRHS(results, solver, items, solution, capacity);
        AnalyzeNewActivity(results, solver, items);
        AnalyzeNewConstraint(results, solver, items);
        AnalyzeShadowPrices(results, items, solution);
        AnalyzeDuality(results, items, solution);

        return results;
    }

    /// <summary>
    /// Analyzes non-basic variables (items not selected in optimal solution)
    /// </summary>
    private static void AnalyzeNonBasicVariables(List<string> results, BranchAndBoundKnapsack solver,
        List<KnapsackItem> items, KnapsackSolution solution, int capacity)
    {
        results.Add("Sensitivity Analysis for Non-Basic Variables:");
        for (int i = 0; i < items.Count; i++)
        {
            if (solution.SelectedItems[i] == 0)
            {
                results.Add($"Variable x{i + 1} is non-basic.");
                DisplayRangeAndApplyChange(results, solver, items, i, solution, capacity);
            }
        }
    }

    /// <summary>
    /// Analyzes basic variables (items selected in optimal solution)
    /// </summary>
    private static void AnalyzeBasicVariables(List<string> results, BranchAndBoundKnapsack solver,
        List<KnapsackItem> items, KnapsackSolution solution, int capacity)
    {
        results.Add("Sensitivity Analysis for Basic Variables:");
        for (int i = 0; i < items.Count; i++)
        {
            if (solution.SelectedItems[i] == 1)
            {
                results.Add($"Variable x{i + 1} is basic.");
                DisplayRangeAndApplyChange(results, solver, items, i, solution, capacity);
            }
        }
    }

    /// <summary>
    /// Analyzes constraint right-hand side values (item weights)
    /// </summary>
    private static void AnalyzeConstraintRHS(List<string> results, BranchAndBoundKnapsack solver,
        List<KnapsackItem> items, KnapsackSolution solution, int capacity)
    {
        results.Add("Sensitivity Analysis for Constraint RHS Values:");
        for (int j = 0; j < items.Count; j++)
        {
            int originalWeight = items[j].Weight;
            items[j].Weight += 1;
            var newSolution = solver.Solve();
            results.Add($"After increasing weight of item {j + 1} by 1, new Objective Value = {newSolution.TotalValue}");
            items[j].Weight = originalWeight;
        }
    }

    /// <summary>
    /// Analyzes the effect of adding a new activity (item)
    /// </summary>
    private static void AnalyzeNewActivity(List<string> results, BranchAndBoundKnapsack solver, List<KnapsackItem> items)
    {
        results.Add("Adding a new activity to the model:");
        items.Add(new KnapsackItem { Weight = 1, Value = 1 });
        var solutionWithNewActivity = solver.Solve();
        results.Add($"Objective Value with new activity = {solutionWithNewActivity.TotalValue}");
        items.RemoveAt(items.Count - 1); // Remove the added item
    }

    /// <summary>
    /// Analyzes the effect of adding a new constraint
    /// </summary>
    private static void AnalyzeNewConstraint(List<string> results, BranchAndBoundKnapsack solver, List<KnapsackItem> items)
    {
        results.Add("Adding a new constraint to the model:");
        if (items.Count > 0)
        {
            items[items.Count - 1].Weight += 1;  // Example constraint adjustment
            var solutionWithNewConstraint = solver.Solve();
            results.Add($"Objective Value with new constraint = {solutionWithNewConstraint.TotalValue}");
            items[items.Count - 1].Weight -= 1;  // Revert change
        }
    }

    /// <summary>
    /// Calculates and analyzes shadow prices
    /// </summary>
    private static void AnalyzeShadowPrices(List<string> results, List<KnapsackItem> items, KnapsackSolution solution)
    {
        results.Add("Shadow Prices:");
        foreach (var item in items)
        {
            results.Add($"Shadow price for item {items.IndexOf(item) + 1}: {CalculateShadowPrice(item, solution.TotalValue)}");
        }
    }

    /// <summary>
    /// Analyzes duality properties of the problem
    /// </summary>
    private static void AnalyzeDuality(List<string> results, List<KnapsackItem> items, KnapsackSolution solution)
    {
        results.Add("Applying Duality:");
        var dualSolution = ApplyDuality(items, out int dualValue);
        results.Add($"Dual Objective Value: {dualValue}");
        results.Add(dualValue == solution.TotalValue ? "Strong Duality" : "Weak Duality");
    }

    /// <summary>
    /// Displays range and applies changes to analyze sensitivity
    /// </summary>
    public static void DisplayRangeAndApplyChange(List<string> results, BranchAndBoundKnapsack solver,
        List<KnapsackItem> items, int variableIndex, KnapsackSolution solution, int capacity)
    {
        int originalValue = items[variableIndex].Value;
        items[variableIndex].Value += 1;
        var newSolution = solver.Solve();
        results.Add($"After increasing value of item {variableIndex + 1} by 1, new Objective Value = {newSolution.TotalValue}");
        items[variableIndex].Value = originalValue;
    }

    /// <summary>
    /// Calculates shadow price for an item
    /// </summary>
    public static int CalculateShadowPrice(KnapsackItem item, int bestValue)
    {
        return item.Weight * bestValue;
    }

    /// <summary>
    /// Applies duality principles to the knapsack problem
    /// </summary>
    public static List<int> ApplyDuality(List<KnapsackItem> items, out int dualValue)
    {
        // Placeholder for dual problem implementation
        dualValue = 0;
        return new List<int>(new int[items.Count]);
    }
}