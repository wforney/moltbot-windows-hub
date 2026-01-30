using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Updatum;

namespace OpenClawTray;

public class DownloadProgressDialog : ModernForm
{
    private readonly UpdatumManager _updater;
    private readonly ProgressBar _progressBar;
    private readonly Label _progressLabel;

    public DownloadProgressDialog(UpdatumManager updater)
    {
        _updater = updater;
        _updater.PropertyChanged += UpdaterOnPropertyChanged;

        Text = "Downloading Update — OpenClaw Tray";
        Size = new Size(420, 160);
        ControlBox = false;
        Icon = IconHelper.GetLobsterIcon();

        var titleLabel = CreateModernLabel("🦞 Downloading update...");
        titleLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        titleLabel.ForeColor = AccentColor;
        titleLabel.Location = new Point(20, 20);
        Controls.Add(titleLabel);

        _progressBar = CreateModernProgressBar();
        _progressBar.Location = new Point(20, 60);
        _progressBar.Size = new Size(364, 8);
        Controls.Add(_progressBar);

        _progressLabel = CreateModernLabel("Starting download...", isSubtle: true);
        _progressLabel.Location = new Point(20, 78);
        _progressLabel.Size = new Size(364, 24);
        _progressLabel.TextAlign = ContentAlignment.MiddleCenter;
        Controls.Add(_progressLabel);
    }

    private void UpdaterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UpdatumManager.DownloadedPercentage))
        {
            if (InvokeRequired)
                Invoke(() => UpdateProgress());
            else
                UpdateProgress();
        }
    }

    private void UpdateProgress()
    {
        _progressBar.Value = (int)Math.Min(_updater.DownloadedPercentage, 100);
        _progressLabel.Text = $"{_updater.DownloadedMegabytes:F2} MB / {_updater.DownloadSizeMegabytes:F2} MB ({_updater.DownloadedPercentage:F1}%)";
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _updater.PropertyChanged -= UpdaterOnPropertyChanged;
        base.OnFormClosing(e);
    }
}

