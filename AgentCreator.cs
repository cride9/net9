using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace net9;

public class AgentCreator
{
    public static AIAgent CreateOllamaAgent(string modelName, string name, string description, ChatOptions options)
    {
        var chatClient = new ChatClient(
            modelName,
            new ApiKeyCredential("ollama"),
            new OpenAIClientOptions { Endpoint = new Uri("http://localhost:11434/v1"), NetworkTimeout = TimeSpan.FromMinutes(10) }
        ).AsIChatClient();

        return new ChatClientAgent(
            chatClient,
            new ChatClientAgentOptions()
            {
                Name = name,
                Description = description,
                ChatOptions = options
            }
        );
    }

    public static AIAgent CreateCustomAgent((string endpoint, string key) apiOptions, string modelName, string name, string description, ChatOptions options)
    {
        var chatClient = new ChatClient(
            modelName,
            new ApiKeyCredential(apiOptions.key),
            new OpenAIClientOptions { Endpoint = new Uri(apiOptions.endpoint), NetworkTimeout = TimeSpan.FromMinutes(10) }
        ).AsIChatClient();

        return new ChatClientAgent(
            chatClient,
            new ChatClientAgentOptions()
            {
                Name = name,
                Description = description,
                ChatOptions = options
            }
        );
    }
}
