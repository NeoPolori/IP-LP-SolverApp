using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Handles file input/output operations for the knapsack problem
/// </summary>
public static class FileHandler
{
    /// <summary>
    /// Reads input file and parses knapsack problem data
    /// </summary>
    /// <param name="filePath">Path to the input file</param>
    /// <returns>Tuple containing capacity, items list, and objective type</returns>
    public static (int, List<KnapsackItem>, string) ReadInputFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length < 2)
        {
            throw new InvalidOperationException("The input file must contain at least two lines.");
        }

        // Extract objective type and item values from the first line
        string[] firstLine = lines[0].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string objectiveType = firstLine[0].ToLower();

        List<KnapsackItem> items = new List<KnapsackItem>();
        for (int i = 1; i < firstLine.Length; i++)
        {
            int value = int.Parse(firstLine[i].TrimStart('+'));
            items.Add(new KnapsackItem { Value = value });
        }

        // Extract weights and capacity from the second line
        string[] constraintLine = lines[1].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < items.Count; i++)
        {
            int weight = int.Parse(constraintLine[i].TrimStart('+'));
            items[i].Weight = weight;
        }

        // Parse capacity from the constraint line
        string capacityStr = constraintLine.Last().Split('=').Last();
        if (!int.TryParse(capacityStr, out int capacity))
        {
            throw new InvalidOperationException("Invalid capacity value in the constraint line.");
        }

        return (capacity, items, objectiveType);
    }

    /// <summary>
    /// Writes sensitivity analysis results to output file
    /// </summary>
    /// <param name="outputFilePath">Path to the output file</param>
    /// <param name="sensitivityAnalysisResults">List of sensitivity analysis results</param>
    public static void WriteSensitivityAnalysisResults(string outputFilePath, List<string> sensitivityAnalysisResults)
    {
        using (var file = new StreamWriter(outputFilePath, true))
        {
            file.WriteLine("\nSensitivity Analysis Results:");
            foreach (var result in sensitivityAnalysisResults)
            {
                file.WriteLine(result);
            }
        }
    }
}