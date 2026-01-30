using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OpenClawTray;

/// <summary>
/// Base form with Windows 11 modern styling - dark/light mode, rounded corners, OpenClaw branding.
/// Inherit from this for consistent look across all dialogs.
/// </summary>
public class ModernForm : Form
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWCP_ROUND = 2;

    // Theme colors - exposed for child controls
    protected bool IsDarkMode { get; private set; }
    protected Color AccentColor => Color.FromArgb(220, 53, 53); // Lobster red
    protected Color BackgroundColor { get; private set; }
    protected Color ForegroundColor { get; private set; }
    protected Color SurfaceColor { get; private set; }
    protected Color BorderColor { get; private set; }
    protected Color HoverColor { get; private set; }
    protected Color SubtleTextColor { get; private set; }

    public ModernForm()
    {
        DetectTheme();
        
        // Base form styling
        Font = new Font("Segoe UI", 9.5f);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
    }

    private void DetectTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            IsDarkMode = value is int i && i == 0;
        }
        catch
        {
            IsDarkMode = false;
        }

        if (IsDarkMode)
        {
            BackgroundColor = Color.FromArgb(32, 32, 32);
            ForegroundColor = Color.FromArgb(255, 255, 255);
            SurfaceColor = Color.FromArgb(45, 45, 48);
            BorderColor = Color.FromArgb(60, 60, 60);
            HoverColor = Color.FromArgb(55, 55, 58);
            SubtleTextColor = Color.FromArgb(180, 180, 180);
        }
        else
        {
            BackgroundColor = Color.FromArgb(249, 249, 249);
            ForegroundColor = Color.FromArgb(28, 28, 28);
            SurfaceColor = Color.FromArgb(255, 255, 255);
            BorderColor = Color.FromArgb(200, 200, 200);
            HoverColor = Color.FromArgb(229, 229, 229);
            SubtleTextColor = Color.FromArgb(100, 100, 100);
        }

        BackColor = BackgroundColor;
        ForeColor = ForegroundColor;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyModernStyling();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // Apply theme colors to all child controls
        ApplyThemeToControls(Controls);
    }

    private void ApplyThemeToControls(Control.ControlCollection controls)
    {
        foreach (Control ctrl in controls)
        {
            // Skip controls that have explicit colors set (like accent-colored labels)
            if (ctrl.ForeColor == AccentColor) continue;
            
            // Apply foreground color to labels and checkboxes
            if (ctrl is Label || ctrl is CheckBox || ctrl is RadioButton)
            {
                if (ctrl.ForeColor == Color.Black || ctrl.ForeColor == SystemColors.ControlText)
                    ctrl.ForeColor = ForegroundColor;
            }
            
            // Recurse into containers
            if (ctrl.HasChildren)
                ApplyThemeToControls(ctrl.Controls);
        }
    }

    private void ApplyModernStyling()
    {
        // Enable Windows 11 rounded corners
        int preference = DWMWCP_ROUND;
        DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));

        // Enable dark mode title bar
        int darkMode = IsDarkMode ? 1 : 0;
        DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
    }

    /// <summary>
    /// Creates a styled button with OpenClaw branding.
    /// </summary>
    protected Button CreateModernButton(string text, bool isPrimary = false)
    {
        var btn = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, isPrimary ? FontStyle.Bold : FontStyle.Regular),
            Cursor = Cursors.Hand,
            Height = 32,
            Padding = new Padding(12, 0, 12, 0)
        };

        if (isPrimary)
        {
            btn.BackColor = AccentColor;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 43, 43);
        }
        else
        {
            btn.BackColor = SurfaceColor;
            btn.ForeColor = ForegroundColor;
            btn.FlatAppearance.BorderColor = BorderColor;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = HoverColor;
        }

        return btn;
    }

    /// <summary>
    /// Creates a styled text box.
    /// </summary>
    protected TextBox CreateModernTextBox()
    {
        return new TextBox
        {
            Font = new Font("Segoe UI", 10f),
            BackColor = SurfaceColor,
            ForeColor = ForegroundColor,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    /// <summary>
    /// Creates a styled label.
    /// </summary>
    protected Label CreateModernLabel(string text, bool isSubtle = false)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = isSubtle ? SubtleTextColor : ForegroundColor,
            AutoSize = true
        };
    }

    /// <summary>
    /// Creates a styled checkbox.
    /// </summary>
    protected CheckBox CreateModernCheckBox(string text)
    {
        var cb = new CheckBox
        {
            Text = text,
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = ForegroundColor,
            BackColor = Color.Transparent,
            AutoSize = true,
            FlatStyle = FlatStyle.Standard
        };
        return cb;
    }

    /// <summary>
    /// Creates a styled group box.
    /// </summary>
    protected GroupBox CreateModernGroupBox(string text)
    {
        return new GroupBox
        {
            Text = text,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = AccentColor,
            BackColor = Color.Transparent
        };
    }

    /// <summary>
    /// Creates a styled panel with border.
    /// </summary>
    protected Panel CreateModernPanel()
    {
        return new Panel
        {
            BackColor = SurfaceColor,
            BorderStyle = BorderStyle.None,
            Padding = new Padding(12)
        };
    }

    /// <summary>
    /// Creates a horizontal separator line.
    /// </summary>
    protected Panel CreateSeparator()
    {
        return new Panel
        {
            Height = 1,
            BackColor = BorderColor,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 8, 0, 8)
        };
    }

    /// <summary>
    /// Creates a styled progress bar.
    /// </summary>
    protected ProgressBar CreateModernProgressBar()
    {
        return new ProgressBar
        {
            Style = ProgressBarStyle.Continuous,
            Height = 6,
            ForeColor = AccentColor
        };
    }
}
