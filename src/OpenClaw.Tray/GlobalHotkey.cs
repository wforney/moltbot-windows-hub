using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenClawTray;

/// <summary>
/// Registers a system-wide hotkey that works even when the app is not focused.
/// Default: Ctrl+Alt+Shift+C to open Quick Send.
/// </summary>
public class GlobalHotkey : IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int HotkeyId = 9001;
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint VK_C = 0x43;

    private readonly HotkeyWindow _window;
    private bool _registered;

    public event EventHandler? HotkeyPressed;

    public GlobalHotkey()
    {
        _window = new HotkeyWindow(this);
    }

    public bool Register()
    {
        try
        {
            _registered = RegisterHotKey(_window.Handle, HotkeyId, MOD_CONTROL | MOD_ALT | MOD_SHIFT, VK_C);
            if (_registered)
                Logger.Info("Global hotkey registered: Ctrl+Alt+Shift+C");
            else
                Logger.Warn("Failed to register global hotkey (may be in use by another app)");
            return _registered;
        }
        catch (Exception ex)
        {
            Logger.Error("Hotkey registration error", ex);
            return false;
        }
    }

    public void Dispose()
    {
        if (_registered)
        {
            UnregisterHotKey(_window.Handle, HotkeyId);
            _registered = false;
        }
        _window.Dispose();
    }

    internal void OnHotkeyPressed()
    {
        HotkeyPressed?.Invoke(this, EventArgs.Empty);
    }

    private class HotkeyWindow : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly GlobalHotkey _owner;

        public HotkeyWindow(GlobalHotkey owner)
        {
            _owner = owner;
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HotkeyId)
            {
                _owner.OnHotkeyPressed();
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }
}

