using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Implements the Branch and Bound algorithm for solving the knapsack problem
/// </summary>
public class BranchAndBoundKnapsack
{
    private int Capacity;
    private List<KnapsackItem> Items;
    private KnapsackSolution BestSolution;
    private StreamWriter Writer;
    private string ObjectiveType;

    /// <summary>
    /// Initializes a new instance of BranchAndBoundKnapsack solver
    /// </summary>
    /// <param name="capacity">Maximum capacity of the knapsack</param>
    /// <param name="items">List of items to consider</param>
    /// <param name="writer">StreamWriter for logging</param>
    /// <param name="objectiveType">"max" for maximization or "min" for minimization</param>
    public BranchAndBoundKnapsack(int capacity, List<KnapsackItem> items, StreamWriter writer, string objectiveType)
    {
        Capacity = capacity;
        Items = items.OrderByDescending(i => i.ValuePerWeight).ToList();
        BestSolution = new KnapsackSolution();
        Writer = writer;
        ObjectiveType = objectiveType.ToLower();
    }

    /// <summary>
    /// Solves the knapsack problem using Branch and Bound algorithm
    /// </summary>
    /// <returns>The optimal solution</returns>
    public KnapsackSolution Solve()
    {
        BranchAndBound(0, 0, 0, new List<int>(new int[Items.Count]));
        return BestSolution;
    }

    /// <summary>
    /// Recursive Branch and Bound method for exploring the solution space
    /// </summary>
    /// <param name="index">Current item index being considered</param>
    /// <param name="currentWeight">Current total weight in knapsack</param>
    /// <param name="currentValue">Current total value in knapsack</param>
    /// <param name="selectedItems">List tracking selected items</param>
    private void BranchAndBound(int index, int currentWeight, int currentValue, List<int> selectedItems)
    {
        // Base case: reached the end of items list
        if (index == Items.Count)
        {
            UpdateBestSolution(currentWeight, currentValue, selectedItems);
            return;
        }

        // Calculate upper bound for the current node
        double bound = CalculateBound(index, currentWeight, currentValue);
        LogCurrentState(index, currentWeight, currentValue, bound, selectedItems);

        // Prune the branch if it cannot lead to a better solution
        if (ShouldPruneBranch(bound))
        {
            return;
        }

        // Explore branch where current item is included
        if (CanIncludeItem(index, currentWeight))
        {
            selectedItems[index] = 1;
            BranchAndBound(index + 1, currentWeight + Items[index].Weight,
                          currentValue + Items[index].Value, selectedItems);
        }

        // Explore branch where current item is excluded
        selectedItems[index] = 0;
        BranchAndBound(index + 1, currentWeight, currentValue, selectedItems);
    }

    /// <summary>
    /// Updates the best solution if current solution is better
    /// </summary>
    private void UpdateBestSolution(int currentWeight, int currentValue, List<int> selectedItems)
    {
        if ((ObjectiveType == "max" && currentWeight <= Capacity && currentValue > BestSolution.TotalValue) ||
            (ObjectiveType == "min" && currentWeight <= Capacity &&
            (BestSolution.TotalValue == 0 || currentValue < BestSolution.TotalValue)))
        {
            BestSolution.TotalValue = currentValue;
            BestSolution.TotalWeight = currentWeight;
            BestSolution.SelectedItems = new List<int>(selectedItems);
        }
    }

    /// <summary>
    /// Logs the current state of the algorithm
    /// </summary>
    private void LogCurrentState(int index, int currentWeight, int currentValue,
                               double bound, List<int> selectedItems)
    {
        Writer.WriteLine($"Index: {index}, Current Weight: {currentWeight}, " +
                        $"Current Value: {currentValue}, Bound: {Math.Round(bound, 3)}, " +
                        $"Selected Items: {string.Join(", ", selectedItems)}");
    }

    /// <summary>
    /// Determines if the current branch should be pruned
    /// </summary>
    private bool ShouldPruneBranch(double bound)
    {
        return (ObjectiveType == "max" && bound <= BestSolution.TotalValue) ||
               (ObjectiveType == "min" && bound >= BestSolution.TotalValue &&
                BestSolution.TotalValue != 0);
    }

    /// <summary>
    /// Checks if the current item can be included without exceeding capacity
    /// </summary>
    private bool CanIncludeItem(int index, int currentWeight)
    {
        return currentWeight + Items[index].Weight <= Capacity;
    }

    /// <summary>
    /// Calculates the upper bound for the current node
    /// </summary>
    /// <returns>The upper bound value</returns>
    private double CalculateBound(int index, int currentWeight, int currentValue)
    {
        double bound = currentValue;
        int totalWeight = currentWeight;

        for (int i = index; i < Items.Count; i++)
        {
            if (totalWeight + Items[i].Weight <= Capacity)
            {
                totalWeight += Items[i].Weight;
                bound += Items[i].Value;
            }
            else
            {
                int remainingWeight = Capacity - totalWeight;
                bound += Items[i].ValuePerWeight * remainingWeight;
                break;
            }
        }
        return bound;
    }
}