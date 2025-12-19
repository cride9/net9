using net9;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

public static class SystemInfoTools
{
    [Description("Gets current date time.")]
    public static string GetCurrentDateTime() {

        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    [Description("Gets OS info.")]
    public static string GetOsInfo() { 

        Logger.Log(MethodBase.GetCurrentMethod()!.Name);
        return $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})"; 
    }
}