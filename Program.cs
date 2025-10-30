using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

using net9;

using OpenAI;
using OpenAI.Chat;

using System.ClientModel;

var chatClient = new ChatClient(
        "deepseek-chat",
        new ApiKeyCredential(Environment.GetEnvironmentVariable("DEEPSEEK")!),
        new OpenAIClientOptions { Endpoint = new Uri("https://api.deepseek.com") }
        ).AsIChatClient();

AIAgent fileSummarizerAgent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions
    {
        Name = "SummarizerAgent",
        Instructions = "You are an agent that only does file summarizing. You'll be given a file, read with the tool and summarize it.",
        ChatOptions = new ChatOptions
        {
            AllowMultipleToolCalls = false,
            ToolMode = ChatToolMode.RequireSpecific("read_file"),
            Tools = new List<AITool>
            {
                new FileRead()
            }
        }
    }
);

AIAgent fileWriterAgent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions
    {
        Name = "GeneralAgent",
        Instructions = Instructions.agentInstruction,
        ChatOptions = new ChatOptions
        {
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = new List<AITool> { 
                new FileWrite(), 
                new StopLoop(),
                fileSummarizerAgent.AsAIFunction()
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
