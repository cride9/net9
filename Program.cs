using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using net9;
using net9.tools;

string sandboxPath = Path.Combine(AppContext.BaseDirectory, "AgentPlayground");
if (!Directory.Exists(sandboxPath)) Directory.CreateDirectory(sandboxPath);

FileSystemTools.SetSandboxPath(sandboxPath);
TerminalTools.SetSandboxPath(sandboxPath);

Dictionary<string, List<AITool>> toolsByCategory = new()
{
    { "FileSystem", new() {
        AIFunctionFactory.Create(FileSystemTools.WriteFile),
        AIFunctionFactory.Create(FileSystemTools.ReadFile),
        AIFunctionFactory.Create(FileSystemTools.ListDirectory),
        AIFunctionFactory.Create(FileSystemTools.FileExists),
        AIFunctionFactory.Create(FileSystemTools.CopyFile),
        AIFunctionFactory.Create(FileSystemTools.MoveFile),
        AIFunctionFactory.Create(FileSystemTools.DeleteFile),
        AIFunctionFactory.Create(FileSystemTools.CreateDirectory),
        AIFunctionFactory.Create(FileSystemTools.SearchFiles),
        AIFunctionFactory.Create(FileSystemTools.ReadPdfFile) // New PDF Tool
    }},
    { "SystemInfo", new() {
        AIFunctionFactory.Create(SystemInfoTools.GetCurrentDateTime),
        AIFunctionFactory.Create(SystemInfoTools.GetOsInfo)
    }},
    { "Terminal", new() {
        AIFunctionFactory.Create(TerminalTools.ExecuteCommand),
        AIFunctionFactory.Create(TerminalTools.RunNpmScript),       // New NPM
        AIFunctionFactory.Create(TerminalTools.InstallNpmPackage),  // New NPM
        AIFunctionFactory.Create(TerminalTools.InitializeWebProject), // New Vite/Scaffold
        AIFunctionFactory.Create(TerminalTools.RunDotnetBuild),
        AIFunctionFactory.Create(TerminalTools.RunDotnetTest)
    }},
    { "Code", new() {
        AIFunctionFactory.Create(CodeTools.AnalyzeCsProject),
        AIFunctionFactory.Create(CodeTools.AddNuGetPackage)
    }},
    { "Network", new() {
        AIFunctionFactory.Create(NetworkTools.GoogleSearch),
        AIFunctionFactory.Create(NetworkTools.ScrapeWebPage),
        AIFunctionFactory.Create(NetworkTools.DownloadFile)
    }},
    { "Memory", new() {
        AIFunctionFactory.Create(MemoryTools.SaveToMemory),
        AIFunctionFactory.Create(MemoryTools.GetFromMemory),
        AIFunctionFactory.Create(MemoryTools.ListMemoryKeys)
    }},
    { "Vision", new() {
        AIFunctionFactory.Create(VisionTools.DescribeImage),
        AIFunctionFactory.Create(VisionTools.CompareImages)
    }}
};

List<AIAgent> agents = new()
{
    AgentCreator.CreateOllamaAgent(
        "qwen3-vl:30b-a3b-instruct-q4_K_M",
        "Vision Agent",
        Instructions.VisionDescription,
        new ChatOptions() { Instructions = Instructions.VisionInstructions }
    ),

    // Developer: Needs everything related to file manipulation, coding, and seeing designs
    AgentCreator.CreateOllamaAgent(
        "qwen3-coder:30b-a3b-q4_K_M",
        "Developer Agent",
        Instructions.DeveloperDescription,
        new ChatOptions() {
            Instructions = Instructions.DeveloperInstructions,
            AllowMultipleToolCalls = true,
            Tools = toolsByCategory["FileSystem"]
                .Union(toolsByCategory["Terminal"])
                .Union(toolsByCategory["Code"])
                .Union(toolsByCategory["Vision"]) // Developer needs eyes to build from mockups
                .ToList()
        }
    ),

    // Reasoning: Pure logic, usually doesn't need external tools, maybe FileRead/Memory
    AgentCreator.CreateOllamaAgent(
        "nemotron-3-nano:30b-a3b-q4_K_M",
        "Reasoning expert",
        Instructions.ReasoningDescription,
        new ChatOptions()
        {
            Instructions = Instructions.ReasoningInstructions,
            AllowMultipleToolCalls = false,
            ToolMode = ChatToolMode.None,
            Temperature = 1f,
            Tools = toolsByCategory["Memory"] // Give it memory access to understand context
        }
    ),

    // Reviewer: Needs to Read files, Analyze Project structure, and Run Builds/Tests (Safely)
    AgentCreator.CreateOllamaAgent(
        "devstral-small-2:24b-instruct-2512-q4_K_M",
        "Code Reviewer",
        Instructions.ReviewerDescription,
        new ChatOptions() {
            Instructions = Instructions.ReviewerInstructions,
            Tools = toolsByCategory["FileSystem"] // Read/List/Search
                .Union(toolsByCategory["Code"])   // Analyze CSProj
                .Union(new List<AITool> {         // Specific Terminal commands only
                    AIFunctionFactory.Create(TerminalTools.RunDotnetBuild),
                    AIFunctionFactory.Create(TerminalTools.RunDotnetTest),
                    AIFunctionFactory.Create(TerminalTools.RunNpmScript)
                })
                .ToList()
        }
    ),

    // Researcher: Needs Network access
    AgentCreator.CreateOllamaAgent(
        "qwen3:4b-instruct-2507-q4_K_M",
        "Researcher Agent",
        Instructions.ResearcherDescription,
        new ChatOptions() {
            Instructions = Instructions.ResearcherInstructions,
            Tools = toolsByCategory["Network"]
        }
    ),
};
VisionTools.Configure(agents.Find(it => it.Name == "Vision Agent")!);

var orchestratorAgent = AgentCreator.CreateOllamaAgent(
    "gpt-oss:20b",
    "Orchestrator Agent",
    Instructions.OrchestratorDescription,
    new ChatOptions()
    {
        Instructions = Instructions.OrchestratorInstructions,
        AllowMultipleToolCalls = true,
        Tools = agents.Select(it => it.AsAIFunction()) // All other agents as tools
            .Cast<AITool>()
            .Union(toolsByCategory["FileSystem"]) // Basic FS access
            .Union(toolsByCategory["SystemInfo"]) // Time/OS info
            .Union(toolsByCategory["Memory"])     // Long-term memory
            .Union(toolsByCategory["Vision"])     // Eyes
            .ToList()
    }
);

var thread = orchestratorAgent.GetNewThread();
Console.Write("Task: ");
var task = Console.ReadLine()!;

try
{
    bool stopStream = false;
    var stream = orchestratorAgent.RunStreamingAsync(task, thread);
    while (!stopStream)
    {
        await foreach (var chunk in stream)
        {
            Console.Write(chunk.Text);
            foreach (var content in chunk.Contents)
            {
                if (content is TextReasoningContent reasonContent)
                    Console.Write(reasonContent.Text);

                if (content is FunctionCallContent fnContent)
                {
                    if (fnContent.Name == "MarkTaskAsComplete")
                        stopStream = true;

                    if (fnContent.Arguments!.ContainsKey("query"))
                    {
                        Logger.Log($"expert_call \"{fnContent.Name}\"");
                        Logger.CurrentAgent = fnContent.Name;
                    }
                    else
                        Logger.CurrentAgent = "Orchestrator Agent";
                }
            }
        }
        stream = orchestratorAgent.RunStreamingAsync(thread);
        Console.WriteLine();
    }

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
Console.ReadKey();