using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

using net9.agents;
using net9.tools;

using OpenAI;
using OpenAI.Chat;

using System.ClientModel;
using System.Runtime.Serialization.Json;

var chatClient = new ChatClient(
        "deepseek-chat",
        new ApiKeyCredential(Environment.GetEnvironmentVariable("DEEPSEEK")!),
        new OpenAIClientOptions { Endpoint = new Uri("https://api.deepseek.com"), NetworkTimeout = TimeSpan.FromMinutes(10) }
).AsIChatClient();

var fileSumAgent = new FileSummariseAgent(chatClient, new List<AITool> { new FileRead() });
var generalAgent = new GeneralAgent(chatClient, 
    new List<AITool> { new FileWrite(), new StopLoop(), fileSumAgent.AsAIFunction() });

Console.Write("Task: ");
var task = Console.ReadLine()!;

AgentThread agentThread = generalAgent.GetNewThread();
await foreach (var chunk in generalAgent.RunStreamingAsync(task, agentThread))
{
    Console.Write(chunk.Text);
}

Console.WriteLine();
while (!StopLoop.StopExecution)
{
    await foreach (var chunk in generalAgent.RunStreamingAsync(agentThread))
    {
        Console.Write(chunk.Text);
    }
    Console.WriteLine();
}
