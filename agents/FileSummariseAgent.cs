using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

using net9.tools;

namespace net9.agents;
public class FileSummariseAgent : AgentTemplate
{
    public FileSummariseAgent(IChatClient client, IList<AITool>? tools = null)
    {
        _agent = new ChatClientAgent(
            client,
            new ChatClientAgentOptions
            {
                Name = "SummarizerAgent",
                Instructions = "You are an agent that only does file summarizing. You'll be given a file, read with the tool and summarize it.",
                ChatOptions = new ChatOptions
                {
                    AllowMultipleToolCalls = false,
                    ToolMode = ChatToolMode.RequireSpecific("read_file"),
                    Tools = tools
                }
            }
        );
    }
}
