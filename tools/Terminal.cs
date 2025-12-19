using net9;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

public static class TerminalTools
{
    private static string _workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Workspace");

    public static void SetSandboxPath(string path)
    {
        _workingDirectory = Path.GetFullPath(path);
    }

    [Description("Executes a terminal command in the workspace.")]
    public static string ExecuteCommand(string command)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = _workingDirectory
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                processStartInfo.FileName = "cmd.exe";
                processStartInfo.Arguments = $"/c {command}";
            }
            else
            {
                processStartInfo.FileName = "/bin/bash";
                processStartInfo.Arguments = $"-c \"{command}\"";
            }

            using var process = new Process { StartInfo = processStartInfo };
            var outSb = new StringBuilder();
            var errSb = new StringBuilder();

            process.OutputDataReceived += (s, e) => { if (e.Data != null) outSb.AppendLine(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) errSb.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (!process.WaitForExit(20000)) // 20s timeout
            {
                process.Kill();
                return $"[TIMEOUT] Process killed. Output:\n{outSb}\nErrors:\n{errSb}";
            }

            return $"Exit Code: {process.ExitCode}\nSTDOUT:\n{outSb}\nSTDERR:\n{errSb}";
        }
        catch (Exception ex) { return $"Error: {ex.Message}"; }
    }

    [Description("Runs an NPM script (e.g., 'npm run build').")]
    public static string RunNpmScript(string scriptName)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        if (scriptName.Contains("dev") || scriptName.Contains("start") || scriptName.Contains("watch"))
            return "Error: Cannot run long-running server scripts. Only build/test.";
        return ExecuteCommand($"npm run {scriptName}");
    }

    [Description("Installs an NPM package.")]
    public static string InstallNpmPackage(string packageName, bool devDependency = false)
    {
        return ExecuteCommand($"npm install {packageName} {(devDependency ? "--save-dev" : "")}");
    }

    [Description("Scaffolds a new project using Vite.")]
    public static string InitializeWebProject(string projectName, string template)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        // Valid templates: react, vue, svelte, vanilla, etc.
        string cmd = $"npm create vite@latest {projectName} -- --template {template}";
        return ExecuteCommand($"{cmd} && cd {projectName} && npm install");
    }

    [Description("Runs 'dotnet build'.")]
    public static string RunDotnetBuild() 
    { 
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        return ExecuteCommand("dotnet build"); 
    }

    [Description("Runs 'dotnet test'.")]
    public static string RunDotnetTest() 
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        return ExecuteCommand("dotnet test"); 
    }
}