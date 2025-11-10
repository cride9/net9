using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace net9.agents;
public class FileSummariseAgent : AgentTemplate
{
    public FileSummariseAgent(IChatClient client, IList<AITool>? tools = null)
    {
        _agent = new ChatClientAgent(
            client,
            new ChatClientAgentOptions
            {
                Name = "FileSummarizer",
                Instructions = "You are an agent that only does file summarizing. You'll be given a file or files, read with the tool and summarize them. If it's a programming code use specification summary",
                ChatOptions = new ChatOptions
                {
                    AllowMultipleToolCalls = true,
                    ToolMode = ChatToolMode.RequireSpecific("read_file"),
                    Tools = tools
                }
            }
        );
    }
}
