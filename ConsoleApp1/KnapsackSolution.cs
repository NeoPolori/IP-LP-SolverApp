using System.Collections.Generic;

/// <summary>
/// Represents the solution to the knapsack problem
/// </summary>
public class KnapsackSolution
{
    /// <summary>
    /// Total value of selected items
    /// </summary>
    public int TotalValue { get; set; }

    /// <summary>
    /// Total weight of selected items
    /// </summary>
    public int TotalWeight { get; set; }

    /// <summary>
    /// List indicating which items are selected (1) or not (0)
    /// </summary>
    public List<int> SelectedItems { get; set; }

    /// <summary>
    /// Initializes a new instance of KnapsackSolution
    /// </summary>
    public KnapsackSolution()
    {
        SelectedItems = new List<int>();
    }
}