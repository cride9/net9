using net9;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

public static class NetworkTools
{
    private static string GoogleApiKey = Environment.GetEnvironmentVariable("google_api")!;
    private static string GoogleSearchEngineId = Environment.GetEnvironmentVariable("google_engine")!;
    private static readonly HttpClient _httpClient = new HttpClient();

    static NetworkTools()
    {
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
    }

    [Description("Searches Google.")]
    public static async Task<string> GoogleSearch(string query)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        if (GoogleApiKey == "YOUR_API_KEY") return "Google API not configured.";
        try
        {
            var url = $"https://www.googleapis.com/customsearch/v1?key={GoogleApiKey}&cx={GoogleSearchEngineId}&q={HttpUtility.UrlEncode(query)}";
            var json = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("items", out var items)) return "No results.";

            return string.Join("\n", items.EnumerateArray().Take(5).Select((item, i) =>
                $"[{i + 1}] {item.GetProperty("title").GetString()}\n    {item.GetProperty("link").GetString()}"));
        }
        catch (Exception ex) { return $"Search Error: {ex.Message}"; }
    }

    [Description("Downloads URL content and strips HTML.")]
    public static async Task<string> ScrapeWebPage(string url)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        try
        {
            var html = await _httpClient.GetStringAsync(url);
            string text = Regex.Replace(html, "<.*?>", " "); // Naive strip
            text = HttpUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return text.Length > 20000 ? text.Substring(0, 20000) + "..." : text;
        }
        catch (Exception ex) { return $"Scrape Error: {ex.Message}"; }
    }

    [Description("Downloads a file from URL.")]
    public static async Task<string> DownloadFile(string url, string savePath)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        try
        {
            var bytes = await _httpClient.GetByteArrayAsync(url);
            // Must use FileSystemTools logic if you want to sandbox this call manually, 
            // or here we assume savePath is relative and let FS tools handle it later?
            // For now, we write directly but users should use FS tools to move it.
            // Better implementation:
            await File.WriteAllBytesAsync(savePath, bytes); // Warning: Not sandboxed here unless injected
            return $"Downloaded {bytes.Length} bytes.";
        }
        catch (Exception ex) { return ex.Message; }
    }
}