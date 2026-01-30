using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OpenClawTray;

public class SettingsManager
{
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OpenClawTray");
    
    private static readonly string SettingsFile = Path.Combine(SettingsDirectory, "settings.json");

    public string GatewayUrl { get; set; } = "ws://localhost:18789";
    public string Token { get; set; } = "";
    public bool AutoStart { get; set; } = false;
    public bool ShowNotifications { get; set; } = true;
    public string NotificationSound { get; set; } = "Default";

    // Notification filters — which types to show toasts for
    public bool NotifyHealth { get; set; } = true;
    public bool NotifyUrgent { get; set; } = true;
    public bool NotifyReminder { get; set; } = true;
    public bool NotifyEmail { get; set; } = true;
    public bool NotifyCalendar { get; set; } = true;
    public bool NotifyBuild { get; set; } = true;
    public bool NotifyStock { get; set; } = true;
    public bool NotifyInfo { get; set; } = true;

    // UI preferences
    public bool ShowGlobalHotkey { get; set; } = true;
    public bool MinimizeToTray { get; set; } = true;

    public SettingsManager()
    {
        Load();
    }

    /// <summary>Check if a notification type should produce a toast.</summary>
    public bool ShouldNotify(string type)
    {
        if (!ShowNotifications) return false;
        return type switch
        {
            "health" => NotifyHealth,
            "urgent" => NotifyUrgent,
            "reminder" => NotifyReminder,
            "email" => NotifyEmail,
            "calendar" => NotifyCalendar,
            "build" => NotifyBuild,
            "stock" => NotifyStock,
            "error" => NotifyUrgent, // Errors use urgent setting
            "info" => NotifyInfo,
            _ => NotifyInfo
        };
    }

    public void Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                var settings = JsonSerializer.Deserialize<SettingsData>(json);
                
                if (settings != null)
                {
                    GatewayUrl = settings.GatewayUrl ?? "ws://localhost:18789";
                    Token = settings.Token ?? "";
                    AutoStart = settings.AutoStart;
                    ShowNotifications = settings.ShowNotifications;
                    NotificationSound = settings.NotificationSound ?? "Default";
                    NotifyHealth = settings.NotifyHealth ?? true;
                    NotifyUrgent = settings.NotifyUrgent ?? true;
                    NotifyReminder = settings.NotifyReminder ?? true;
                    NotifyEmail = settings.NotifyEmail ?? true;
                    NotifyCalendar = settings.NotifyCalendar ?? true;
                    NotifyBuild = settings.NotifyBuild ?? true;
                    NotifyStock = settings.NotifyStock ?? true;
                    NotifyInfo = settings.NotifyInfo ?? true;
                    ShowGlobalHotkey = settings.ShowGlobalHotkey ?? true;
                    MinimizeToTray = settings.MinimizeToTray ?? true;
                }
            }
        }
        catch (Exception)
        {
            // Use defaults if loading fails
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            
            var settings = new SettingsData
            {
                GatewayUrl = GatewayUrl,
                Token = Token,
                AutoStart = AutoStart,
                ShowNotifications = ShowNotifications,
                NotificationSound = NotificationSound,
                NotifyHealth = NotifyHealth,
                NotifyUrgent = NotifyUrgent,
                NotifyReminder = NotifyReminder,
                NotifyEmail = NotifyEmail,
                NotifyCalendar = NotifyCalendar,
                NotifyBuild = NotifyBuild,
                NotifyStock = NotifyStock,
                NotifyInfo = NotifyInfo,
                ShowGlobalHotkey = ShowGlobalHotkey,
                MinimizeToTray = MinimizeToTray
            };

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
            Logger.Info("Settings saved");
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to save settings", ex);
            throw new Exception($"Failed to save settings: {ex.Message}", ex);
        }
    }

    public static string GetSettingsDirectory() => SettingsDirectory;
    
    public static string GetSettingsFile() => SettingsFile;

    private class SettingsData
    {
        public string? GatewayUrl { get; set; }
        public string? Token { get; set; }
        public bool AutoStart { get; set; }
        public bool ShowNotifications { get; set; }
        public string? NotificationSound { get; set; }
        public bool? NotifyHealth { get; set; }
        public bool? NotifyUrgent { get; set; }
        public bool? NotifyReminder { get; set; }
        public bool? NotifyEmail { get; set; }
        public bool? NotifyCalendar { get; set; }
        public bool? NotifyBuild { get; set; }
        public bool? NotifyStock { get; set; }
        public bool? NotifyInfo { get; set; }
        public bool? ShowGlobalHotkey { get; set; }
        public bool? MinimizeToTray { get; set; }
    }
}
