using Microsoft.Extensions.AI;

using System.Text.Json;

namespace net9.tools;

public class StopLoop : AIFunction
{
    public override string Name => "stop_loop";
    public override string Description => "This function will stop the run loop. When you decide your task is done. Call this to stop the running";
    public static bool StopExecution = false;

    public override JsonElement JsonSchema => JsonDocument.Parse(@"
        {
            ""type"": ""object"",
            ""properties"": {
                ""stop"": {
                    ""type"": ""boolean"",
                    ""description"": ""True - Stopping, False - Continue""
                }
            },
            ""required"": [""stop""]
        }").RootElement;

    protected override async ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments,
        CancellationToken cancellationToken)
    {

        StopExecution = arguments.GetValueOrDefault("stop") is JsonElement stopTemp ? stopTemp.GetBoolean() : false;
        return StopExecution ? "Execution stopping..." : "Continuing execution...";
    }
}
