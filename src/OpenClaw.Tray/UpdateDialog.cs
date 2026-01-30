using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenClawTray;

public enum UpdateDialogResult
{
    Download,
    RemindLater,
    Skip
}

public class UpdateDialog : ModernForm
{
    public UpdateDialogResult Result { get; private set; } = UpdateDialogResult.RemindLater;

    public UpdateDialog(string version, string releaseNotes)
    {
        Text = "Update Available — OpenClaw Tray";
        Size = new Size(500, 420);
        Icon = IconHelper.GetLobsterIcon();

        var titleLabel = CreateModernLabel("🦞 Update Available!");
        titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        titleLabel.ForeColor = AccentColor;
        titleLabel.Location = new Point(20, 20);
        Controls.Add(titleLabel);

        var versionLabel = CreateModernLabel($"Version {version} is ready to install");
        versionLabel.Location = new Point(20, 55);
        Controls.Add(versionLabel);

        var notesLabel = CreateModernLabel("Release Notes:");
        notesLabel.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        notesLabel.Location = new Point(20, 90);
        Controls.Add(notesLabel);

        var notesBox = CreateModernTextBox();
        notesBox.Text = string.IsNullOrWhiteSpace(releaseNotes) ? "No release notes available." : releaseNotes;
        notesBox.Multiline = true;
        notesBox.ReadOnly = true;
        notesBox.ScrollBars = ScrollBars.Vertical;
        notesBox.Location = new Point(20, 115);
        notesBox.Size = new Size(444, 200);
        Controls.Add(notesBox);

        var skipButton = CreateModernButton("Skip Version");
        skipButton.Size = new Size(120, 36);
        skipButton.Location = new Point(20, 330);
        skipButton.Click += (_, _) =>
        {
            Result = UpdateDialogResult.Skip;
            DialogResult = DialogResult.Cancel;
            Close();
        };
        Controls.Add(skipButton);

        var remindButton = CreateModernButton("Remind Later");
        remindButton.Size = new Size(120, 36);
        remindButton.Location = new Point(230, 330);
        remindButton.Click += (_, _) =>
        {
            Result = UpdateDialogResult.RemindLater;
            DialogResult = DialogResult.Cancel;
            Close();
        };
        Controls.Add(remindButton);

        var downloadButton = CreateModernButton("Download && Install", isPrimary: true);
        downloadButton.Size = new Size(140, 36);
        downloadButton.Location = new Point(324, 330);
        downloadButton.Click += (_, _) =>
        {
            Result = UpdateDialogResult.Download;
            DialogResult = DialogResult.OK;
            Close();
        };
        Controls.Add(downloadButton);

        AcceptButton = downloadButton;
        CancelButton = remindButton;
    }
}

