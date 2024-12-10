using SantaClausRealtimeChat.Models;
using System.Text.Json;

namespace SantaClausRealtimeChat.Helpers;

/// <summary>
///     Provides helper methods for file operations related to wish items.
/// </summary>
internal static class FileHelper
{
    private static readonly JsonSerializerOptions jsonSerializerOptions
        = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Reads the wish items from a JSON file.
    /// </summary>
    /// <returns>A list of wish items if the file is found and deserialized 
    /// successfully; otherwise, null.</returns>
    public static List<WishItem>? ReadWishItems()
    {
        try
        {
            var filePath = FindFile("Assets/wishes.json");
            var fileContent = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<List<WishItem>>(fileContent, jsonSerializerOptions);
        }
        catch (FileNotFoundException ex)
        {
            ConsoleHelper.DisplayError($"File not found: {ex.Message}", true);
            return null;
        }
        catch (JsonException ex)
        {
            ConsoleHelper.DisplayError($"Error deserializing JSON: {ex.Message}", true);
            return null;
        }
    }

    /// <summary>
    /// Finds the specified file by searching the current directory and its 
    /// parent directories.
    /// </summary>
    /// <param name="fileName">The name of the file to find.</param>
    /// <returns>The full path of the file if found.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
    private static string FindFile(string fileName)
    {
        for (string currentDirectory = Directory.GetCurrentDirectory();
             currentDirectory != null && currentDirectory != Path.GetPathRoot(currentDirectory);
             currentDirectory = Directory.GetParent(currentDirectory)?.FullName!)
        {
            string filePath = Path.Combine(currentDirectory, fileName);
            if (File.Exists(filePath))
            {
                return filePath;
            }
        }

        throw new FileNotFoundException($"File '{fileName}' not found.");
    }
}
