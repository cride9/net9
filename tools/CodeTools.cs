using net9;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

public static class CodeTools
{
    [Description("Adds a NuGet package.")]
    public static string AddNuGetPackage(string projectPath, string packageName, string version = "")
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        string args = $"add \"{projectPath}\" package {packageName}" + (string.IsNullOrEmpty(version) ? "" : $" -v {version}");
        return RunProcess("dotnet", args);
    }

    [Description("Analyzes a .csproj file.")]
    public static string AnalyzeCsProject(string projectPath)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        try
        {
            if (!File.Exists(projectPath)) return "Project not found.";
            var doc = XDocument.Load(projectPath);
            var tf = doc.Descendants("TargetFramework").FirstOrDefault()?.Value ?? "Unknown";
            var pkgs = doc.Descendants("PackageReference").Select(x => $"{x.Attribute("Include")?.Value} ({x.Attribute("Version")?.Value})");
            return $"Framework: {tf}\nPackages:\n{string.Join("\n", pkgs)}";
        }
        catch (Exception ex) { return ex.Message; }
    }

    private static string RunProcess(string exe, string args)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        var p = new Process { StartInfo = new ProcessStartInfo { FileName = exe, Arguments = args, RedirectStandardOutput = true, UseShellExecute = false } };
        p.Start();
        p.WaitForExit(10000);
        return p.StandardOutput.ReadToEnd();
    }
}