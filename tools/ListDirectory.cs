using Microsoft.Extensions.AI;

using System.Text.Json;

namespace net9.tools;
public class ListDirectory : AIFunction
{
    public override string Name => "list_files";
    public override string Description => "Return every file and folder name inside the working folder";
    public override JsonElement JsonSchema => JsonDocument.Parse(@"
        {
            ""type"": ""object"",
            ""properties"": {},
            ""required"": []
        }").RootElement;

    protected override ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        string outputDirectory = Path.Combine(Directory.GetCurrentDirectory( ), "GeneratedFiles");

        var files = Directory.GetFiles(outputDirectory)
            .Select(Path.GetFileName)
            .ToArray( );

        var folders = Directory.GetDirectories(outputDirectory)
            .Select(Path.GetFileName)
            .ToArray( );

        var result = new
        {
            directory = outputDirectory,
            files,
            folders,
            message = "ok"
        };
        Console.WriteLine($"\n[TOOL CALL] List directories");
        JsonElement json = JsonSerializer.SerializeToElement(result);
        return new ValueTask<object?>(json);
    }
}
