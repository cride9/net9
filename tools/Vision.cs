using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Reflection;

namespace net9.tools;

public static class VisionTools
{
    private static AIAgent? _visionClient;

    public static void Configure(AIAgent client)
    {
        _visionClient = client;
    }

    [Description("Analyzes a local image file and describes what is seen.")]
    public static async Task<string> DescribeImage(
        [Description("The absolute path to the image file.")] string imagePath,
        [Description("The specific question (e.g., 'What is the error in this screenshot?').")] string prompt = "Describe this image in detail.")
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        if (_visionClient == null) return "Error: VisionTools not configured. Call Configure() first.";
        if (!File.Exists(imagePath)) return $"Error: Image not found at {imagePath}";

        try
        {
            string dataUri = GetDataUri(imagePath);

            var message = new ChatMessage(ChatRole.User, new List<AIContent>
            {
                new TextContent(prompt),
                new UriContent(dataUri, "image/jpeg")
            });

            var response = await _visionClient.RunAsync(message);
            return response.ToString();
        }
        catch (Exception ex)
        {
            return $"Error analyzing image: {ex.Message}";
        }
    }

    [Description("Compares two images (e.g., a design mockup vs. the actual screenshot).")]
    public static async Task<string> CompareImages(
        [Description("Path to the first image (the expectation/mockup).")] string path1,
        [Description("Path to the second image (the reality/screenshot).")] string path2)
    {
        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        if (_visionClient == null) return "Error: VisionTools not configured.";
        if (!File.Exists(path1) || !File.Exists(path2)) return "Error: One or both image files not found.";

        try
        {
            string dataUri1 = GetDataUri(path1);
            string dataUri2 = GetDataUri(path2);

            var message = new ChatMessage(ChatRole.User, new List<AIContent>
            {
                new TextContent("Compare these two images. Describe the differences in layout, styling, and content."),
                new UriContent(dataUri1, "image/jpeg"),
                new UriContent(dataUri2, "image/jpeg")
            });

            var response = await _visionClient.RunAsync(message);
            return response.ToString();
        }
        catch (Exception ex)
        {
            return $"Error comparing images: {ex.Message}";
        }
    }

    private static string GetDataUri(string filePath)
    {
        byte[] imageBytes = File.ReadAllBytes(filePath);
        string base64 = Convert.ToBase64String(imageBytes);
        return $"data:image/jpeg;base64,{base64}";
    }
}