using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace OpenClawTray;

/// <summary>
/// Shows recent notification history in a modern styled list view.
/// </summary>
public class NotificationHistoryForm : ModernForm
{
    private ListView? _listView;
    private Button _clearButton = null!;
    private Button _closeButton = null!;
    private static NotificationHistoryForm? _instance;

    private static readonly List<NotificationEntry> _history = new();
    private const int MaxHistory = 200;

    public static void AddEntry(string title, string message, string type)
    {
        lock (_history)
        {
            _history.Add(new NotificationEntry
            {
                Timestamp = DateTime.Now,
                Title = title,
                Message = message,
                Type = type
            });

            while (_history.Count > MaxHistory)
                _history.RemoveAt(0);
        }

        _instance?.RefreshList();
    }

    public static void ShowOrFocus()
    {
        if (_instance != null && !_instance.IsDisposed)
        {
            _instance.BringToFront();
            _instance.Focus();
            return;
        }

        _instance = new NotificationHistoryForm();
        _instance.Show();
    }

    private NotificationHistoryForm()
    {
        InitializeComponent();
        RefreshList();
    }

    private void InitializeComponent()
    {
        Text = "Notification History — OpenClaw Tray";
        Size = new Size(680, 500);
        MinimumSize = new Size(480, 340);
        FormBorderStyle = FormBorderStyle.Sizable;
        Icon = IconHelper.GetLobsterIcon();

        _listView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = false,
            Font = new Font("Segoe UI", 9.5F),
            BackColor = SurfaceColor,
            ForeColor = ForegroundColor,
            BorderStyle = BorderStyle.None
        };
        _listView.Columns.Add("Time", 140);
        _listView.Columns.Add("Type", 85);
        _listView.Columns.Add("Title", 160);
        _listView.Columns.Add("Message", 320);

        var buttonPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            BackColor = SurfaceColor,
            Padding = new Padding(16, 12, 16, 12)
        };

        _closeButton = CreateModernButton("Close");
        _closeButton.Size = new Size(90, 36);
        _closeButton.Click += (_, _) => Close();

        _clearButton = CreateModernButton("Clear All", isPrimary: true);
        _clearButton.Size = new Size(100, 36);
        _clearButton.Click += (_, _) =>
        {
            lock (_history) _history.Clear();
            RefreshList();
        };

        var buttonFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            BackColor = Color.Transparent
        };
        buttonFlow.Controls.Add(_closeButton);
        buttonFlow.Controls.Add(_clearButton);
        
        buttonPanel.Controls.Add(buttonFlow);

        buttonPanel.Controls.Add(_closeButton);
        buttonPanel.Controls.Add(_clearButton);

        Controls.Add(_listView);
        Controls.Add(buttonPanel);
    }

    private void RefreshList()
    {
        if (_listView == null || _listView.IsDisposed) return;

        if (InvokeRequired)
        {
            Invoke(new Action(RefreshList));
            return;
        }

        _listView.BeginUpdate();
        _listView.Items.Clear();

        lock (_history)
        {
            for (int i = _history.Count - 1; i >= 0; i--)
            {
                var entry = _history[i];
                var item = new ListViewItem(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                item.SubItems.Add(entry.Type);
                item.SubItems.Add(entry.Title);
                item.SubItems.Add(entry.Message.Replace('\n', ' '));
                _listView.Items.Add(item);
            }
        }

        _listView.EndUpdate();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _instance = null;
        base.OnFormClosed(e);
    }

    private class NotificationEntry
    {
        public DateTime Timestamp { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Type { get; set; } = "";
    }
}


