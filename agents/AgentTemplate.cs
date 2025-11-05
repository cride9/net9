using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

using System.Text.Json;

namespace net9.agents;
public class AgentTemplate : AIAgent
{
    protected AIAgent _agent;

    public AIFunction AsAIFunction()
    {
        return _agent.AsAIFunction();
    }

    public override AgentThread DeserializeThread(JsonElement serializedThread, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return _agent.DeserializeThread(serializedThread, jsonSerializerOptions);
    }

    public override AgentThread GetNewThread( )
    {
        return _agent.GetNewThread( );
    }

    public override Task<AgentRunResponse> RunAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return _agent.RunAsync(messages, thread, options, cancellationToken);
    }

    public IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(string task, AgentThread thread)
    {
        return _agent.RunStreamingAsync(task, thread);
    }

    public IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(string task)
    {
        return _agent.RunStreamingAsync(task);
    }
    public IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(AgentThread thread)
    {
        return _agent.RunStreamingAsync(thread);
    }

    public override IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return _agent.RunStreamingAsync(messages, thread, options, cancellationToken);
    }
}
