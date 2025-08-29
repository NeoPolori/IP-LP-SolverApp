using System;

/// <summary>
/// Represents an item in the knapsack problem with weight and value properties
/// </summary>
public class KnapsackItem
{
    /// <summary>
    /// Weight of the item
    /// </summary>
    public int Weight { get; set; }

    /// <summary>
    /// Value of the item
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Calculated value per weight ratio for sorting and bounding
    /// </summary>
    public double ValuePerWeight => Math.Round((double)Value / Weight, 3);
}