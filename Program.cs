using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

using net9;

using OpenAI;
using OpenAI.Chat;

using System.ClientModel;

IChatClient chatClient =
    new ChatClient(
        "deepseek-chat",
        new ApiKeyCredential("sk-[REDACTED]"),
        new OpenAIClientOptions { Endpoint = new Uri("https://api.deepseek.com") }
        ).AsIChatClient();

AIAgent writer = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions
    {
        Name = "Writer",
        Instructions = "A helpful assistant with equipped tools",
        ChatOptions = new ChatOptions
        {
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = new List<AITool>() { new FileWrite(), new FileRead() }
        }
    }
    );

await foreach (var chunk in writer.RunStreamingAsync("Write a short story about a haunted house. Filename : \"story.txt\""))
{
    Console.Write(chunk.Text);
}

Console.ReadKey();