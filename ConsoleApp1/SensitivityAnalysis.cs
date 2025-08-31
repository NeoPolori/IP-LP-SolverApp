using System;
using System.Collections.Generic;


// Performs sensitivity analysis on the knapsack solution
public static class SensitivityAnalysis
{
    // Performs comprehensive sensitivity analysis on the knapsack solution
    public static List<string> PerformSensitivityAnalysis(BranchAndBoundKnapsack solver,
        List<KnapsackItem> items, KnapsackSolution solution, int capacity)
    {
        var results = new List<string>();

        // Run different sensitivity analysis methods
        AnalyzeNonBasicVariables(results, solver, items, solution, capacity);
        AnalyzeBasicVariables(results, solver, items, solution, capacity);
        AnalyzeConstraintRHS(results, solver, items, solution, capacity);
        AnalyzeNewActivity(results, solver, items);
        AnalyzeNewConstraint(results, solver, items);
        AnalyzeShadowPrices(results, items, solution);
        AnalyzeDuality(results, items, solution);

        return results;
    }


    // Analyzes non-basic variables (items NOT selected in the optimal solution)
    private static void AnalyzeNonBasicVariables(List<string> results, BranchAndBoundKnapsack solver,
        List<KnapsackItem> items, KnapsackSolution solution, int capacity)
    {
        results.Add("Sensitivity Analysis for Non-Basic Variables:");
        for (int i = 0; i < items.Count; i++)
        {
            if (solution.SelectedItems[i] == 0) // If item was not chosen
            {
                results.Add($"Variable x{i + 1} is non-basic.");
                // Try changing its value to see effect on the solution
                DisplayRangeAndApplyChange(results, solver, items, i, solution, capacity);
            }
        }
    }

    // Analyzes basic variables (items selected in the optimal solution)
    private static void AnalyzeBasicVariables(List<string> results, BranchAndBoundKnapsack solver,
        List<KnapsackItem> items, KnapsackSolution solution, int capacity)
    {
        results.Add("Sensitivity Analysis for Basic Variables:");
        for (int i = 0; i < items.Count; i++)
        {
            if (solution.SelectedItems[i] == 1) // If item was chosen
            {
                results.Add($"Variable x{i + 1} is basic.");
                // Try changing its value to see effect on the solution
                DisplayRangeAndApplyChange(results, solver, items, i, solution, capacity);
            }
        }
    }


    /// Analyzes sensitivity of RHS (Right-Hand Side) constraints = item weights
    private static void AnalyzeConstraintRHS(List<string> results, BranchAndBoundKnapsack solver,
        List<KnapsackItem> items, KnapsackSolution solution, int capacity)
    {
        results.Add("Sensitivity Analysis for Constraint RHS Values:");
        for (int j = 0; j < items.Count; j++)
        {
            int originalWeight = items[j].Weight; // Store original weight
            items[j].Weight += 1; // Slightly increase weight
            var newSolution = solver.Solve(); // Solve again
            results.Add($"After increasing weight of item {j + 1} by 1, new Objective Value = {newSolution.TotalValue}");
            items[j].Weight = originalWeight; // Revert change
        }
    }

    // Analyzes the effect of adding a new item (new activity)
    private static void AnalyzeNewActivity(List<string> results, BranchAndBoundKnapsack solver, List<KnapsackItem> items)
    {
        results.Add("Adding a new activity to the model:");
        items.Add(new KnapsackItem { Weight = 1, Value = 1 }); // Add a dummy item
        var solutionWithNewActivity = solver.Solve();
        results.Add($"Objective Value with new activity = {solutionWithNewActivity.TotalValue}");
        items.RemoveAt(items.Count - 1); // Remove test item
    }

    // Analyzes the effect of adding a new constraint to the model
    private static void AnalyzeNewConstraint(List<string> results, BranchAndBoundKnapsack solver, List<KnapsackItem> items)
    {
        results.Add("Adding a new constraint to the model:");
        if (items.Count > 0)
        {
            items[items.Count - 1].Weight += 1;  // Example: increase last item's weight (mimics new constraint)
            var solutionWithNewConstraint = solver.Solve();
            results.Add($"Objective Value with new constraint = {solutionWithNewConstraint.TotalValue}");
            items[items.Count - 1].Weight -= 1;  // Revert change
        }
    }

    // Estimates and displays shadow prices (how much an item contributes per unit of resource)
    private static void AnalyzeShadowPrices(List<string> results, List<KnapsackItem> items, KnapsackSolution solution)
    {
        results.Add("Shadow Prices:");
        foreach (var item in items)
        {
            results.Add($"Shadow price for item {items.IndexOf(item) + 1}: {CalculateShadowPrice(item, solution.TotalValue)}");
        }
    }


    // Checks duality properties (relation between primal and dual problems)
    private static void AnalyzeDuality(List<string> results, List<KnapsackItem> items, KnapsackSolution solution)
    {
        results.Add("Applying Duality:");
        var dualSolution = ApplyDuality(items, out int dualValue);
        results.Add($"Dual Objective Value: {dualValue}");
        results.Add(dualValue == solution.TotalValue ? "Strong Duality" : "Weak Duality");
    }


    // Helper method: temporarily increases value of an item to check sensitivity
    public static void DisplayRangeAndApplyChange(List<string> results, BranchAndBoundKnapsack solver,
        List<KnapsackItem> items, int variableIndex, KnapsackSolution solution, int capacity)
    {
        int originalValue = items[variableIndex].Value; // Store old value
        items[variableIndex].Value += 1; // Increase item value
        var newSolution = solver.Solve(); // Solve again
        results.Add($"After increasing value of item {variableIndex + 1} by 1, new Objective Value = {newSolution.TotalValue}");
        items[variableIndex].Value = originalValue; // Revert back
    }

    // Simple calculation of shadow price (placeholder logic)
    public static int CalculateShadowPrice(KnapsackItem item, int bestValue)
    {
        return item.Weight * bestValue;
    }

    // Placeholder: Applies duality to the knapsack problem
    public static List<int> ApplyDuality(List<KnapsackItem> items, out int dualValue)
    {
        //Implement actual dual problem logic
        dualValue = 0; // Currently always zero
        return new List<int>(new int[items.Count]); // Returns dummy dual solution
    }
}
