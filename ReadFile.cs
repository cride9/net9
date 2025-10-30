using Microsoft.Extensions.AI;

using System.Text.Json;

namespace net9;
public class FileRead : AIFunction
{
    public override string Name => "read_file";
    public override string Description => "Reads the entire content of a specified text file and returns it as a string. Not suitable for binary files like images or PDFs.";

    public override JsonElement JsonSchema => JsonDocument.Parse(@"
        {
            ""type"": ""object"",
            ""properties"": {
                ""name"": {
                    ""type"": ""string"",
                    ""description"": ""The file's name without the extension, e.g., 'story', 'my-notes'""
                },
                ""extension"": {
                    ""type"": ""string"",
                    ""description"": ""The file's extension, e.g., '.txt', '.md'""
                }
            },
            ""required"": [""name"", ""extension""]
        }").RootElement;

    protected override async ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments,
        CancellationToken cancellationToken)
    {
       
        string? name = (arguments.GetValueOrDefault("name") is JsonElement nameElem) ? nameElem.GetString() : null;
        string? extension = (arguments.GetValueOrDefault("extension") is JsonElement extensionElem) ? extensionElem.GetString() : null;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(extension))
        {
            return "Error: Both file name and extension must be provided.";
        }
        if (!extension.StartsWith("."))
        {
            extension = "." + extension;
        }

        try
        {
            string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedFiles");
            string fileName = name + extension;
            string fullPath = Path.GetFullPath(fileName, outputDirectory);

            if (!fullPath.StartsWith(Path.GetFullPath(outputDirectory)))
            {
                return $"Error: Invalid file path. Files can only be read from the '{outputDirectory}' folder.";
            }

            if (!File.Exists(fullPath))
            {
                return $"Error: The file '{fileName}' was not found in the '{outputDirectory}' directory.";
            }

            Console.WriteLine($"\n[TOOL CALL] Reading file: {fullPath}");
            string content = await File.ReadAllTextAsync(fullPath, cancellationToken);

            return content;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[TOOL ERROR] Failed to read file: {ex.Message}");
            Console.ResetColor();

            return $"Error: Could not read the file. Reason: {ex.Message}";
        }
    }
}