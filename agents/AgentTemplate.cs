using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace net9.agents;
public class AgentTemplate
{
    protected AIAgent _agent;
    public AIFunction AsAIFunction()
    {
        return _agent.AsAIFunction();
    }

    public AgentThread GetNewThread()
    {
        return _agent.GetNewThread();
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
}
