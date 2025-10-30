using Microsoft.Extensions.AI;
using System.Text.Json;

namespace net9;
public class FileWrite : AIFunction
{
    public override string Name => "write_file";
    public override string Description => "Can create a raw text file. The tool is not designed to work with PDF or other encoded filetypes";
    public override JsonElement JsonSchema => JsonDocument.Parse(@"
        {
            ""type"": ""object"",
            ""properties"": {
                ""name"": {
                    ""type"": ""string"",
                    ""description"": ""The file's name""
                },
                ""extension"": {
                    ""type"": ""string"",
                    ""description"": ""The file's extension eg.: .txt, .md""
                },
                ""content"": {
                    ""type"": ""string"",
                    ""description"": ""The file's content that has to be written into it""
                },
                ""append"": {
                    ""type"": ""boolean"",
                    ""description"": ""Decides if a file should be appended or overwritten""
                }
            },
            ""required"": [""name"", ""extension"", ""content"", ""append""]
        }").RootElement;

    protected override async ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments,
        CancellationToken cancellationToken)
    {
        // 2. Cast to JsonElement and then get the primitive value
        string? name = (arguments.GetValueOrDefault("name") is JsonElement nameElem) ? nameElem.GetString() : null;
        string? extension = (arguments.GetValueOrDefault("extension") is JsonElement extElem) ? extElem.GetString() : null;
        string? content = (arguments.GetValueOrDefault("content") is JsonElement contentElem) ? contentElem.GetString() : null;
        bool append = (arguments.GetValueOrDefault("append") is JsonElement appendElem) ? appendElem.GetBoolean() : false;

        // 2. Validate the inputs
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Error: File name cannot be empty.";
        }
        if (string.IsNullOrWhiteSpace(extension))
        {
            return "Error: File extension cannot be empty.";
        }
        if (!extension.StartsWith("."))
        {
            extension = "." + extension;
        }

        try
        {
            string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "GeneratedFiles");
            Directory.CreateDirectory(outputDirectory); // This does nothing if the directory already exists

            string fileName = Path.GetFullPath(name + extension, outputDirectory);

            if (!fileName.StartsWith(Path.GetFullPath(outputDirectory)))
            {
                return $"Error: Invalid file path. Files can only be written to the '{outputDirectory}' folder.";
            }

            if (append)
            {
                Console.WriteLine($"\n[TOOL CALL] Appending to file: {fileName}");
                await File.AppendAllTextAsync(fileName, content, cancellationToken);
                return $"Success: Content appended to {fileName}.";
            }
            else
            {
                Console.WriteLine($"\n[TOOL CALL] Writing to file: {fileName}");
                await File.WriteAllTextAsync(fileName, content, cancellationToken);
                return $"Success: File '{fileName}' created/overwritten.";
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[TOOL ERROR] Failed to write file: {ex.Message}");
            Console.ResetColor();

            return $"Error: Could not write to the file. Reason: {ex.Message}";
        }
    }
}
