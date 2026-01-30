using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OpenClawTray;

/// <summary>
/// Embeds the OpenClaw WebChat UI via WebView2 with modern Windows 11 styling.
/// </summary>
public class WebChatForm : ModernForm
{
    private WebView2? _webView;
    private readonly string _gatewayUrl;
    private readonly string _token;
    private Panel? _toolbar;
    private bool _initialized;

    private static WebChatForm? _instance;

    public static void ShowOrFocus(string gatewayUrl, string token)
    {
        if (_instance != null && !_instance.IsDisposed)
        {
            _instance.BringToFront();
            _instance.Focus();
            return;
        }

        _instance = new WebChatForm(gatewayUrl, token);
        _instance.Show();
    }

    private WebChatForm(string gatewayUrl, string token)
    {
        _gatewayUrl = gatewayUrl;
        _token = token;
        InitializeComponent();
        _ = InitializeWebViewAsync();
    }

    private void InitializeComponent()
    {
        Text = "OpenClaw Chat";
        Size = new Size(520, 750);
        MinimumSize = new Size(380, 450);
        FormBorderStyle = FormBorderStyle.Sizable;
        Icon = IconHelper.GetLobsterIcon();

        // Modern toolbar panel - generous height for emoji rendering
        _toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = SurfaceColor
        };

        var btnY = 8;
        var homeBtn = CreateToolbarButton("🏠", "Home");
        homeBtn.Location = new Point(8, btnY);
        homeBtn.Click += (_, _) => NavigateToChat();

        var refreshBtn = CreateToolbarButton("↻", "Refresh");
        refreshBtn.Location = new Point(50, btnY);
        refreshBtn.Click += (_, _) => _webView?.Reload();

        var popoutBtn = CreateToolbarButton("↗", "Open in Browser");
        popoutBtn.Location = new Point(92, btnY);
        popoutBtn.Click += (_, _) =>
        {
            var url = _gatewayUrl.Replace("ws://", "http://").Replace("wss://", "https://");
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo($"{url}?token={Uri.EscapeDataString(_token)}") { UseShellExecute = true }); }
            catch { }
        };

        var devToolsBtn = CreateToolbarButton("🔧", "DevTools");
        devToolsBtn.Location = new Point(134, btnY);
        devToolsBtn.Click += (_, _) => _webView?.CoreWebView2?.OpenDevToolsWindow();

        _toolbar.Controls.AddRange(new Control[] { homeBtn, refreshBtn, popoutBtn, devToolsBtn });

        // WebView2 fills remaining space
        _webView = new WebView2
        {
            Dock = DockStyle.Fill,
            DefaultBackgroundColor = IsDarkMode ? Color.FromArgb(25, 25, 25) : Color.FromArgb(250, 250, 250)
        };

        Controls.Add(_webView);
        Controls.Add(_toolbar);
    }

    private Button CreateToolbarButton(string icon, string tooltip)
    {
        var btn = new Button
        {
            Text = icon,
            Size = new Size(38, 34),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Symbol", 12),
            Cursor = Cursors.Hand,
            BackColor = Color.Transparent,
            ForeColor = ForegroundColor,
            UseCompatibleTextRendering = true
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = HoverColor;
        
        var toolTip = new ToolTip();
        toolTip.SetToolTip(btn, tooltip);
        
        return btn;
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            var userDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "OpenClawTray", "WebView2");

            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataDir);
            await _webView!.EnsureCoreWebView2Async(env);

            var settings = _webView.CoreWebView2.Settings;
            settings.IsStatusBarEnabled = false;
            settings.AreDefaultContextMenusEnabled = true;
            settings.IsZoomControlEnabled = true;

            _initialized = true;
            Logger.Info("WebView2 initialized");
            NavigateToChat();
        }
        catch (WebView2RuntimeNotFoundException)
        {
            Logger.Error("WebView2 runtime not found");
            var result = MessageBox.Show(
                "The Microsoft WebView2 Runtime is required for the chat panel.\n\n" +
                "Would you like to download it?",
                "WebView2 Required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                    "https://developer.microsoft.com/en-us/microsoft-edge/webview2/")
                { UseShellExecute = true });
            }
            Close();
        }
        catch (Exception ex)
        {
            Logger.Error("WebView2 init failed", ex);
            MessageBox.Show($"Failed to initialize chat panel:\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private void NavigateToChat()
    {
        if (!_initialized || _webView?.CoreWebView2 == null) return;

        var httpUrl = _gatewayUrl
            .Replace("ws://", "http://")
            .Replace("wss://", "https://");

        var chatUrl = $"{httpUrl}?token={Uri.EscapeDataString(_token)}";
        _webView.CoreWebView2.Navigate(chatUrl);
        Logger.Info($"Navigating to WebChat: {httpUrl}");
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _webView?.Dispose();
        _instance = null;
        base.OnFormClosed(e);
    }
}


