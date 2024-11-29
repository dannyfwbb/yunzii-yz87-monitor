using Microsoft.Win32;
using System.Diagnostics;

namespace YZ87Monitor.Helpers;

internal static class RegistryHelper
{
    private const string AppName = "Yunzii YZ87 Battery Monitor";
    private const string StartupRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsStartupEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, writable: false);
        return key?.GetValue(AppName) != null;
    }

    public static void AddToStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, writable: true)
                            ?? Registry.CurrentUser.CreateSubKey(StartupRegistryPath);
            var executablePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (executablePath != null)
            {
                key.SetValue(AppName, executablePath);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to add to startup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void RemoveFromStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, writable: true);
            key?.DeleteValue(AppName, throwOnMissingValue: false);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to remove from startup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
