using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

public class MainItemDatabaseImporter : EditorWindow
{
    private string csvFilePath = "";
    private MainItemDatabase database;

    [MenuItem("Tools/Import Main Item Database")]
    public static void ShowWindow()
    {
        GetWindow<MainItemDatabaseImporter>("Main Item Database Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import CSV to Main Item Database", EditorStyles.boldLabel);

        csvFilePath = EditorGUILayout.TextField("CSV File Path:", csvFilePath);
        if (GUILayout.Button("Browse"))
        {
            csvFilePath = EditorUtility.OpenFilePanel("Select CSV file", "", "csv");
        }

        database = (MainItemDatabase)EditorGUILayout.ObjectField("Main Item Database:", database, typeof(MainItemDatabase), false);

        if (GUILayout.Button("Import"))
        {
            if (string.IsNullOrEmpty(csvFilePath) || database == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select both a CSV file and a Main Item Database", "OK");
                return;
            }

            ImportCSV();
        }
    }

    private void ImportCSV()
    {
        string[] lines = File.ReadAllLines(csvFilePath);
        database.items.Clear();

        // Skip the header line
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Use a more robust CSV parsing approach
            List<string> values = ParseCSVLine(line);

            // Check if we have the correct number of values
            if (values.Count != 10)
            {
                Debug.LogError($"Error on line {i + 1}: Incorrect number of values. Expected 10, got {values.Count}. Line content: {line}");
                continue;
            }

            try
            {
                MainItemData item = new MainItemData
                {
                    ID = ParseIntSafe(values[0], "ID"),
                    Name = values[1],
                    Type = values[2],
                    kg = ParseIntSafe(values[3], "kg"),
                    hand = values[4],
                    sound = values[5].ToUpper() == "Y",
                    interact = values[6].ToUpper() == "Y",
                    Conduction = values[7].ToUpper() == "Y",
                    buy = ParseIntSafeNullable(values[8], "buy"),
                    gold = ParseVector2IntSafeNullable(values[9])
                };

                database.items.Add(item);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing line {i + 1}: {line}\nError: {ex.Message}");
                continue;
            }
        }

        if (database.items.Count > 0)
        {
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Import Successful", $"CSV data has been imported to the Main Item Database. {database.items.Count} items were imported.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Import Failed", "No items were imported. Please check the CSV file format and try again.", "OK");
        }
    }

    private List<string> ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        StringBuilder field = new StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(field.ToString().Trim());
                field.Clear();
            }
            else
            {
                field.Append(c);
            }
        }

        result.Add(field.ToString().Trim());
        return result;
    }

    private int ParseIntSafe(string value, string fieldName)
    {
        value = value.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new FormatException($"{fieldName} cannot be empty");
        }
        if (int.TryParse(value, out int result))
        {
            return result;
        }
        throw new FormatException($"Invalid {fieldName} value: {value}");
    }

    private int? ParseIntSafeNullable(string value, string fieldName)
    {
        value = value.Trim();
        if (value.ToLower() == "null" || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        if (int.TryParse(value, out int result))
        {
            return result;
        }
        throw new FormatException($"Invalid {fieldName} value: {value}");
    }

    private Vector2Int? ParseVector2IntSafeNullable(string value)
    {
        value = value.Trim();
        if (value.ToLower() == "null" || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        string[] parts = value.Trim('(', ')').Split(',');
        if (parts.Length != 2)
        {
            throw new FormatException($"Invalid Vector2Int format: {value}");
        }
        return new Vector2Int(ParseIntSafe(parts[0].Trim(), "Vector2Int.x"), ParseIntSafe(parts[1].Trim(), "Vector2Int.y"));
    }
}