using System.ComponentModel;
using System.Reflection;
namespace net9;

public class AgentTaskCompletedException : Exception
{
    public string FinalResult { get; }
    public AgentTaskCompletedException(string result) : base("Task Completed")
    {
        FinalResult = result;
    }
}

public static class ControlFlowTools
{
    [Description("Call this function IMMEDIATELY when the user's request is fully satisfied. This stops the process and returns the final answer.")]
    public static string MarkTaskAsComplete(
        [Description("The final summary or answer to show to the user.")] string finalResult)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        throw new AgentTaskCompletedException(finalResult);
    }
}