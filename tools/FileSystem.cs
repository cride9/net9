using net9;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using UglyToad.PdfPig; // Requires NuGet: UglyToad.PdfPig

public static class FileSystemTools
{
    private static string _sandboxRoot = Path.Combine(Directory.GetCurrentDirectory(), "Workspace");

    public static void SetSandboxPath(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        _sandboxRoot = Path.GetFullPath(path);
    }

    private static string GetSafePath(string inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || inputPath == ".") return _sandboxRoot;
        string fullPath = Path.GetFullPath(Path.Combine(_sandboxRoot, inputPath));
        if (!fullPath.StartsWith(_sandboxRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException($"Access Denied: Path '{inputPath}' is outside the sandbox.");
        }
        return fullPath;
    }

    [Description("Lists all files and subdirectories.")]
    public static string ListDirectory([Description("Relative path.")] string path = ".")
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        try
        {
            string safePath = GetSafePath(path);
            if (!Directory.Exists(safePath)) return $"Directory not found: {path}";

            var files = Directory.GetFiles(safePath);
            var dirs = Directory.GetDirectories(safePath);
            var sb = new StringBuilder();
            sb.AppendLine($"Contents of ./{Path.GetRelativePath(_sandboxRoot, safePath)}:");
            sb.AppendLine("[Directories]");
            foreach (var d in dirs) sb.AppendLine($"  {Path.GetFileName(d)}/");
            sb.AppendLine("[Files]");
            foreach (var f in files) sb.AppendLine($"  {Path.GetFileName(f)}");
            return sb.ToString();
        }
        catch (Exception ex) { return $"Error: {ex.Message}"; }
    }

    [Description("Reads a file.")]
    public static string ReadFile(string filePath)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        try
        {
            return File.ReadAllText(GetSafePath(filePath));
        }
        catch (Exception ex) { return $"Error: {ex.Message}"; }
    }

    [Description("Writes content to a file.")]
    public static string WriteFile(string filePath, string content)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        try
        {
            string safePath = GetSafePath(filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(safePath)!);
            File.WriteAllText(safePath, content);
            return $"File saved to {filePath}";
        }
        catch (Exception ex) { return $"Error: {ex.Message}"; }
    }

    [Description("Checks if a file exists.")]
    public static string FileExists(string path) {

        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        return File.Exists(GetSafePath(path)) ? "True" : "False"; 
    }

    [Description("Deletes a file.")]
    public static string DeleteFile(string path)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        try { File.Delete(GetSafePath(path)); return "Deleted."; } catch (Exception ex) { return ex.Message; }
    }

    [Description("Creates a directory.")]
    public static string CreateDirectory(string path)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        try { Directory.CreateDirectory(GetSafePath(path)); return "Created."; } catch (Exception ex) { return ex.Message; }
    }

    [Description("Moves a file.")]
    public static string MoveFile(string source, string dest)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        try { File.Move(GetSafePath(source), GetSafePath(dest)); return "Moved."; } catch (Exception ex) { return ex.Message; }
    }

    [Description("Copies a file.")]
    public static string CopyFile(string source, string dest)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        try { File.Copy(GetSafePath(source), GetSafePath(dest), true); return "Copied."; } catch (Exception ex) { return ex.Message; }
    }

    [Description("Recursively searches for files matching a pattern.")]
    public static string SearchFiles(string rootPath, string pattern)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        try
        {
            string safePath = GetSafePath(rootPath);
            var files = Directory.GetFiles(safePath, pattern, SearchOption.AllDirectories);
            if (files.Length == 0) return "No matches.";
            // Return relative paths
            return string.Join("\n", files.Take(50).Select(f => Path.GetRelativePath(_sandboxRoot, f)));
        }
        catch (Exception ex) { return ex.Message; }
    }

    [Description("Reads text from a PDF file.")]
    public static string ReadPdfFile(string filePath, int? pageNumber = null)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        try
        {
            string safePath = GetSafePath(filePath);
            if (!File.Exists(safePath)) return "File not found.";

            using var stream = File.OpenRead(safePath);
            using var document = PdfDocument.Open(stream);

            if (pageNumber.HasValue)
            {
                if (pageNumber.Value > 0 && pageNumber.Value <= document.NumberOfPages)
                    return document.GetPage(pageNumber.Value).Text;
                return $"Error: Page {pageNumber} not found (Total: {document.NumberOfPages})";
            }

            var sb = new StringBuilder();
            foreach (var page in document.GetPages().Take(50)) // Limit to 50 pages
            {
                sb.AppendLine(page.Text);
                sb.AppendLine($"--- End of Page {page.Number} ---");
            }
            return sb.ToString();
        }
        catch (Exception ex) { return $"PDF Error: {ex.Message}"; }
    }
}