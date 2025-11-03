using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using net9.agents;
using net9.tools;

using OpenAI;
using OpenAI.Chat;

using System.ClientModel;

// chat client létrehozás, ez lesz az AI api végpontja
// Jelen esetben deepseek API van használva
var chatClient = new ChatClient(
        "deepseek-chat",
        new ApiKeyCredential(Environment.GetEnvironmentVariable("DEEPSEEK")!),
        new OpenAIClientOptions { Endpoint = new Uri("https://api.deepseek.com"), NetworkTimeout = TimeSpan.FromMinutes(10) }
).AsIChatClient();

// egyszerűsített classba foglalt Agent definiálás
var fileSumAgent = new FileSummariseAgent(chatClient, 
    new List<AITool> { new FileRead() });

var generalAgent = new GeneralAgent(chatClient, 
    new List<AITool> { new FileWrite(), new StopLoop(), fileSumAgent.AsAIFunction() });

// Task bekérése
Console.Write("Task: ");
var task = Console.ReadLine()!;

// AgentThread kezdés hogy az Agent memóriája megmaradjon
AgentThread agentThread = generalAgent.GetNewThread();

// Itt most streaming van használva, hogy látszódjon a valós idejű generálás
await foreach (var chunk in generalAgent.RunStreamingAsync(task, agentThread))
{
    Console.Write(chunk.Text);
}

Console.WriteLine(); // Sor kihagyás

// ReAct loop futni fog amíg az AI nem állítja le azt a "StopLoop" toolal
while (!StopLoop.StopExecution)
{
    await foreach (var chunk in generalAgent.RunStreamingAsync(agentThread))
    {
        Console.Write(chunk.Text);
    }
    Console.WriteLine();
}
