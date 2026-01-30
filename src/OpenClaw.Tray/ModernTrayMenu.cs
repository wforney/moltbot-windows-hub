using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OpenClawTray;

/// <summary>
/// Modern flyout menu with Windows 11 styling - dark/light mode, rounded corners, acrylic blur.
/// Replaces the dated ContextMenuStrip with a custom-drawn popup.
/// </summary>
public class ModernTrayMenu : Form
{
    // DWM APIs for acrylic/mica effect
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS { public int Left, Right, Top, Bottom; }

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    private const int DWMWCP_ROUND = 2;
    private const int DWMSBT_TRANSIENTWINDOW = 3; // Acrylic

    // Theme colors
    private bool _isDarkMode;
    private Color _backgroundColor;
    private Color _foregroundColor;
    private Color _hoverColor;
    private Color _accentColor;
    private Color _separatorColor;
    private Color _subtleTextColor;

    // Menu items
    private readonly List<ModernMenuItem> _items = new();
    private int _hoveredIndex = -1;
    private const int ItemHeight = 36;
    private const int IconWidth = 32;  // Wider for emoji
    private const int Padding = 16;    // More padding
    private const int CornerRadius = 8;

    private readonly ToolTip _toolTip = new() { InitialDelay = 400, ReshowDelay = 100 };
    private int _lastTooltipIndex = -1;

    public event EventHandler<string>? MenuItemClicked;

    public ModernTrayMenu()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;

        // Detect theme (styling applied in OnHandleCreated)
        DetectTheme();

        // Track mouse for hover effects
        MouseMove += OnMouseMove;
        MouseLeave += (_, _) => { _hoveredIndex = -1; Invalidate(); };
        MouseClick += OnMouseClick;

        // Close when clicking outside
        Deactivate += (_, _) => Hide();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyModernStyling();
    }

    private void DetectTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            _isDarkMode = value is int i && i == 0;
        }
        catch
        {
            _isDarkMode = false;
        }

        if (_isDarkMode)
        {
            _backgroundColor = Color.FromArgb(32, 32, 32);
            _foregroundColor = Color.FromArgb(255, 255, 255);
            _hoverColor = Color.FromArgb(45, 45, 48);
            _accentColor = Color.FromArgb(255, 99, 71); // Lobster red
            _separatorColor = Color.FromArgb(80, 80, 80);
            _subtleTextColor = Color.FromArgb(180, 180, 180);
        }
        else
        {
            _backgroundColor = Color.FromArgb(249, 249, 249);
            _foregroundColor = Color.FromArgb(28, 28, 28);
            _hoverColor = Color.FromArgb(229, 229, 229);
            _accentColor = Color.FromArgb(220, 53, 53); // Lobster red
            _separatorColor = Color.FromArgb(200, 200, 200);
            _subtleTextColor = Color.FromArgb(100, 100, 100);
        }

        BackColor = _backgroundColor;
    }

    private void ApplyModernStyling()
    {
        // Enable Windows 11 rounded corners
        int preference = DWMWCP_ROUND;
        DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));

        // Enable dark mode for title bar (affects some rendering)
        int darkMode = _isDarkMode ? 1 : 0;
        DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

        // Try to enable acrylic backdrop (Windows 11 22H2+)
        int backdropType = DWMSBT_TRANSIENTWINDOW;
        DwmSetWindowAttribute(Handle, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
    }

    public void ClearItems() => _items.Clear();

    public void AddBrandHeader(string icon, string text, string? tooltip = null)
    {
        _items.Add(new ModernMenuItem
        {
            Id = "",
            Icon = icon,
            Text = text,
            Enabled = false,
            IsHeader = true,
            IsBrandHeader = true,
            IsSeparator = false,
            Tooltip = tooltip
        });
    }

    public void AddItem(string id, string icon, string text, bool enabled = true, bool isHeader = false)
    {
        _items.Add(new ModernMenuItem
        {
            Id = id,
            Icon = icon,
            Text = text,
            Enabled = enabled,
            IsHeader = isHeader,
            IsSeparator = false
        });
    }

    public void AddSeparator()
    {
        _items.Add(new ModernMenuItem { IsSeparator = true });
    }

    public void AddStatusItem(string id, string icon, string text, string status, Color statusColor)
    {
        _items.Add(new ModernMenuItem
        {
            Id = id,
            Icon = icon,
            Text = text,
            Status = status,
            StatusColor = statusColor,
            Enabled = true
        });
    }

    public void ShowAtCursor()
    {
        // Calculate size
        int width = 320;  // Wider for better spacing
        int height = Padding * 2;
        foreach (var item in _items)
        {
            if (item.IsSeparator)
                height += 9;
            else if (item.IsBrandHeader)
                height += 48;  // Big brand header
            else if (item.IsHeader)
                height += 32;
            else
                height += ItemHeight;
        }

        // Minimum height if no items
        if (height < 50) height = 50;

        Size = new Size(width, height);

        // Position near cursor, but keep on screen
        var cursor = Cursor.Position;
        var screen = Screen.FromPoint(cursor).WorkingArea;

        int x = cursor.X - width / 2;
        int y = cursor.Y - height - 10;

        // Adjust if off screen
        if (x < screen.Left) x = screen.Left + 8;
        if (x + width > screen.Right) x = screen.Right - width - 8;
        if (y < screen.Top) y = cursor.Y + 20; // Show below cursor instead
        if (y + height > screen.Bottom) y = screen.Bottom - height - 8;

        Location = new Point(x, y);
        Show();
        Activate();
        Invalidate(); // Force repaint
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // Draw rounded background
        using var bgBrush = new SolidBrush(_backgroundColor);
        using var path = CreateRoundedRectangle(ClientRectangle, CornerRadius);
        g.FillPath(bgBrush, path);

        // Draw border
        using var borderPen = new Pen(Color.FromArgb(_isDarkMode ? 50 : 30, _isDarkMode ? 255 : 0, _isDarkMode ? 255 : 0, _isDarkMode ? 255 : 0), 1);
        g.DrawPath(borderPen, path);

        // Draw items
        int y = Padding;
        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            
            if (item.IsSeparator)
            {
                // Draw separator line
                using var sepPen = new Pen(_separatorColor, 1);
                g.DrawLine(sepPen, Padding, y + 4, Width - Padding, y + 4);
                y += 9;
                continue;
            }

            int itemHeight;
            if (item.IsBrandHeader)
                itemHeight = 48;
            else if (item.IsHeader)
                itemHeight = 32;
            else
                itemHeight = ItemHeight;
                
            var itemRect = new Rectangle(8, y, Width - 16, itemHeight);

            // Hover highlight
            if (i == _hoveredIndex && item.Enabled && !item.IsHeader)
            {
                using var hoverBrush = new SolidBrush(_hoverColor);
                using var hoverPath = CreateRoundedRectangle(itemRect, 4);
                g.FillPath(hoverBrush, hoverPath);
            }

            // Icon - special handling for brand header
            if (!string.IsNullOrEmpty(item.Icon))
            {
                Color iconColor;
                float iconFontSize;
                string fontName;
                int iconWidth;
                
                if (item.IsBrandHeader)
                {
                    iconColor = _accentColor;
                    iconFontSize = 26;  // Big lobster!
                    fontName = "Segoe UI Emoji";  // Use emoji font for lobster
                    iconWidth = 60;  // Plenty of room for lobster
                }
                else if (item.IsHeader)
                {
                    iconColor = _accentColor;
                    iconFontSize = 14;
                    fontName = "Segoe UI Symbol";
                    iconWidth = IconWidth;
                }
                else if (!item.Enabled || string.IsNullOrEmpty(item.Id) || item.Id.StartsWith("session:"))
                {
                    iconColor = _subtleTextColor;
                    iconFontSize = 11;
                    fontName = "Segoe UI Symbol";
                    iconWidth = IconWidth;
                }
                else
                {
                    iconColor = _accentColor;
                    iconFontSize = 11;
                    fontName = "Segoe UI Symbol";
                    iconWidth = IconWidth;
                }
                
                using var iconFont = new Font(fontName, iconFontSize);
                var iconRect = new Rectangle(Padding, y, iconWidth, itemHeight);
                TextRenderer.DrawText(g, item.Icon, iconFont, iconRect, iconColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }

            // Text
            var textColor = item.IsHeader ? _foregroundColor : (item.Enabled ? _foregroundColor : _subtleTextColor);
            var fontSize = item.IsBrandHeader ? 14f : (item.IsHeader ? 10.5f : 9.5f);
            var fontStyle = (item.IsHeader || item.IsBrandHeader) ? FontStyle.Bold : FontStyle.Regular;
            using var textFont = new Font("Segoe UI", fontSize, fontStyle);
            var textX = Padding + (item.IsBrandHeader ? 64 : IconWidth + 4);
            // Only reserve space for status badge if item has one
            var rightMargin = string.IsNullOrEmpty(item.Status) ? Padding : 70;
            var textRect = new Rectangle(textX, y, Width - textX - rightMargin, itemHeight);
            TextRenderer.DrawText(g, item.Text, textFont, textRect, textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            // Status badge (right side)
            if (!string.IsNullOrEmpty(item.Status))
            {
                using var statusFont = new Font("Segoe UI", 8, FontStyle.Bold);
                var statusSize = TextRenderer.MeasureText(item.Status, statusFont);
                var statusRect = new Rectangle(Width - Padding - statusSize.Width - 12, y + (itemHeight - 18) / 2, statusSize.Width + 8, 18);
                
                using var statusBgBrush = new SolidBrush(Color.FromArgb(30, item.StatusColor));
                using var statusPath = CreateRoundedRectangle(statusRect, 4);
                g.FillPath(statusBgBrush, statusPath);
                
                TextRenderer.DrawText(g, item.Status, statusFont, statusRect, item.StatusColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            y += itemHeight;
        }
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        int y = Padding;
        int newHover = -1;
        int tooltipIndex = -1;

        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            int itemHeight;
            if (item.IsSeparator)
                itemHeight = 9;
            else if (item.IsBrandHeader)
                itemHeight = 48;
            else if (item.IsHeader)
                itemHeight = 32;
            else
                itemHeight = ItemHeight;

            // Check if mouse is over this item
            if (e.Y >= y && e.Y < y + itemHeight)
            {
                // Show tooltip for brand header
                if (item.IsBrandHeader && !string.IsNullOrEmpty(item.Tooltip))
                {
                    tooltipIndex = i;
                }
            }

            // Allow hover on non-separators that are either:
            // - Not headers and enabled, OR
            // - Headers with an ID (clickable headers like Sessions)
            var isClickable = !item.IsSeparator && !item.IsBrandHeader && 
                ((!item.IsHeader && item.Enabled) || (item.IsHeader && !string.IsNullOrEmpty(item.Id)));
            
            if (isClickable)
            {
                if (e.Y >= y && e.Y < y + itemHeight)
                {
                    newHover = i;
                }
            }
            y += itemHeight;
        }

        // Update tooltip
        if (tooltipIndex != _lastTooltipIndex)
        {
            _lastTooltipIndex = tooltipIndex;
            if (tooltipIndex >= 0)
            {
                _toolTip.SetToolTip(this, _items[tooltipIndex].Tooltip);
            }
            else
            {
                _toolTip.SetToolTip(this, null);
            }
        }

        if (newHover != _hoveredIndex)
        {
            _hoveredIndex = newHover;
            Cursor = newHover >= 0 ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }
    }

    private void OnMouseClick(object? sender, MouseEventArgs e)
    {
        if (_hoveredIndex >= 0 && _hoveredIndex < _items.Count)
        {
            var item = _items[_hoveredIndex];
            // Allow clicking if enabled, not separator, and either not a header OR a header with an ID
            if (item.Enabled && !item.IsSeparator && (!item.IsHeader || !string.IsNullOrEmpty(item.Id)))
            {
                Hide();
                MenuItemClicked?.Invoke(this, item.Id);
            }
        }
    }

    private static GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int diameter = radius * 2;
        var arc = new Rectangle(rect.X, rect.Y, diameter, diameter);

        path.AddArc(arc, 180, 90); // Top-left
        arc.X = rect.Right - diameter;
        path.AddArc(arc, 270, 90); // Top-right
        arc.Y = rect.Bottom - diameter;
        path.AddArc(arc, 0, 90); // Bottom-right
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90); // Bottom-left
        path.CloseFigure();

        return path;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW
            return cp;
        }
    }

    private class ModernMenuItem
    {
        public string Id { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Text { get; set; } = "";
        public string Status { get; set; } = "";
        public Color StatusColor { get; set; } = Color.Gray;
        public bool Enabled { get; set; } = true;
        public bool IsSeparator { get; set; }
        public bool IsHeader { get; set; }
        public bool IsBrandHeader { get; set; }
        public string? Tooltip { get; set; }
    }
}
