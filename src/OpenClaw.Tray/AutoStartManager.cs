using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace OpenClawTray;

public static class AutoStartManager
{
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string ApplicationName = "OpenClawTray";

    public static void SetAutoStart(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key != null)
            {
                if (enabled)
                {
                    var exePath = GetExecutablePath();
                    key.SetValue(ApplicationName, $"\"{exePath}\"");
                }
                else
                {
                    key.DeleteValue(ApplicationName, false);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to update auto-start setting: {ex.Message}", 
                          "Auto-start Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    public static bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            return key?.GetValue(ApplicationName) != null;
        }
        catch
        {
            return false;
        }
    }

    private static string GetExecutablePath()
    {
        // Use ProcessPath for single-file deployments (Assembly.Location is empty)
        var location = Environment.ProcessPath ?? Application.ExecutablePath;

        return location;
    }
}
