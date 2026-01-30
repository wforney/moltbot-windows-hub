using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace OpenClawTray;

/// <summary>
/// First-run welcome dialog to help users get started with OpenClaw.
/// </summary>
public class WelcomeDialog : ModernForm
{
    private readonly string _dashboardUrl;

    public WelcomeDialog(string dashboardUrl)
    {
        _dashboardUrl = dashboardUrl;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Welcome to Molty";
        Size = new Size(500, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Icon = IconHelper.GetLobsterIcon();

        var y = 20;

        // Lobster header
        var headerLabel = new Label
        {
            Text = "🦞",
            Font = new Font("Segoe UI Emoji", 36),
            Location = new Point(0, y),
            Size = new Size(ClientSize.Width, 60),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = AccentColor
        };
        y += 70;

        // Welcome text
        var welcomeLabel = new Label
        {
            Text = "Welcome to Molty!",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(0, y),
            Size = new Size(ClientSize.Width, 30),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = ForegroundColor,
            BackColor = Color.Transparent
        };
        y += 40;

        // Instructions
        var instructionsLabel = CreateModernLabel(
            "To get started, you'll need an API token from your\n" +
            "OpenClaw dashboard. Click below to learn how to get one,\n" +
            "then paste your token in Settings.");
        instructionsLabel.Font = new Font("Segoe UI", 9.5f);
        instructionsLabel.Location = new Point(30, y);
        instructionsLabel.Size = new Size(ClientSize.Width - 60, 60);
        instructionsLabel.TextAlign = ContentAlignment.MiddleCenter;
        y += 85;

        // Learn about tokens button
        var learnBtn = CreateModernButton("Learn How to Get a Token", isPrimary: true);
        learnBtn.Location = new Point((ClientSize.Width - 250) / 2, y);
        learnBtn.Size = new Size(250, 40);
        learnBtn.Click += (_, _) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://docs.molt.bot/web/dashboard") { UseShellExecute = true });
            }
            catch { }
        };
        y += 55;

        // Open Settings button
        var settingsBtn = CreateModernButton("Open Settings");
        settingsBtn.Location = new Point((ClientSize.Width - 160) / 2, y);
        settingsBtn.Size = new Size(160, 36);
        settingsBtn.Click += (_, _) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };

        Controls.AddRange(new Control[] { headerLabel, welcomeLabel, instructionsLabel, learnBtn, settingsBtn });
    }
}
