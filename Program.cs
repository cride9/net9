using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

using net9;

using OpenAI;
using OpenAI.Chat;

using System.ClientModel;

AIAgent fileWriterAgent = new ChatClientAgent(
    new ChatClient(
        "gpt-oss:20b",
        new ApiKeyCredential("ollama"),
        new OpenAIClientOptions { Endpoint = new Uri("http://localhost:11434/v1") }
        ).AsIChatClient(),
    new ChatClientAgentOptions
    {
        Name = "Writer",
        Instructions = Instructions.agentInstruction,
        ChatOptions = new ChatOptions
        {
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = new List<AITool>() { 
                new FileWrite(), 
                new FileRead(), 
                new StopLoop() 
            },
        },
        ChatMessageStoreFactory = context => new InMemoryChatMessageStore(),
    }
    );

Console.Write("Task: ");
var task = Console.ReadLine();
if (string.IsNullOrWhiteSpace(task))
{
    Console.WriteLine("Bad task");
    return;
}

AgentThread agentThread = fileWriterAgent.GetNewThread();
await foreach (var chunk in fileWriterAgent.RunStreamingAsync(task, agentThread))
{
    Console.Write(chunk.Text);
}

Console.WriteLine();

while (!StopLoop.StopExecution)
{
    await foreach (var chunk in fileWriterAgent.RunStreamingAsync(agentThread))
    {
        Console.Write(chunk.Text);
    }
    Console.WriteLine();
}
