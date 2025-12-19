using net9;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

public static class MemoryTools
{
    private static readonly ConcurrentDictionary<string, string> _mem = new();

    [Description("Saves info to memory.")]
    public static string SaveToMemory(string key, string value)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        _mem[key] = value;
        return "Saved.";
    }

    [Description("Gets info from memory.")]
    public static string GetFromMemory(string key) { 
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        return _mem.TryGetValue(key, out var v) ? v : "Not found."; 
    }

    [Description("Lists memory keys.")]
    public static string ListMemoryKeys() { 
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        return string.Join(", ", _mem.Keys); 
    }
}