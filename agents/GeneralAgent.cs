using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace net9.agents;
public class GeneralAgent : AgentTemplate
{
    public GeneralAgent(IChatClient client, IList<AITool>? tools = null)
    {
        _agent = new ChatClientAgent(
            client,
            new ChatClientAgentOptions
            {
                Name = "GeneralAgent",
                Instructions = Instructions.agentInstruction,
                ChatOptions = new ChatOptions
                {
                    AllowMultipleToolCalls = true,
                    ToolMode = ChatToolMode.Auto,
                    Tools = tools,
                },
            }
        );
    }
}
