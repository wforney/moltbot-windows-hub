using System;
using System.Runtime.InteropServices;

namespace OpenClawTray.Services;

/// <summary>
/// Registers and handles global hotkeys using P/Invoke.
/// Default: Ctrl+Alt+Shift+C for Quick Send.
/// </summary>
public class GlobalHotkeyService : IDisposable
{
    private const int HOTKEY_ID = 9001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_SHIFT = 0x0004;
    private const uint VK_C = 0x43;
    private const int WM_HOTKEY = 0x0312;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName,
        uint dwStyle, int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll")]
    private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private const uint WM_QUIT = 0x0012;
    private const uint WM_USER = 0x0400;

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct WNDCLASS
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string? lpszMenuName;
        public string lpszClassName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    private IntPtr _hwnd;
    private bool _registered;
    private bool _disposed;
    private Thread? _messageThread;
    private WndProcDelegate? _wndProcDelegate; // prevent GC collection
    private volatile bool _running;

    public event EventHandler? HotkeyPressed;

    public GlobalHotkeyService()
    {
    }

    public bool Register()
    {
        if (_registered) return true;

        try
        {
            // Create message window on a dedicated thread with message loop
            _running = true;
            _messageThread = new Thread(MessageLoop) { IsBackground = true, Name = "HotkeyMessageLoop" };
            _messageThread.Start();

            // Wait briefly for window creation
            Thread.Sleep(100);

            if (_hwnd == IntPtr.Zero)
            {
                Logger.Warn("Failed to create hotkey message window");
                return false;
            }

            _registered = RegisterHotKey(_hwnd, HOTKEY_ID, MOD_CONTROL | MOD_ALT | MOD_SHIFT, VK_C);
            if (_registered)
            {
                Logger.Info("Global hotkey registered: Ctrl+Alt+Shift+C");
            }
            else
            {
                Logger.Warn("Failed to register global hotkey (may be in use by another app)");
            }
            return _registered;
        }
        catch (Exception ex)
        {
            Logger.Error($"Hotkey registration error: {ex.Message}");
            return false;
        }
    }

    private void MessageLoop()
    {
        try
        {
            // Create window class
            _wndProcDelegate = WndProc;
            var wndClass = new WNDCLASS
            {
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
                hInstance = GetModuleHandle(null),
                lpszClassName = "OpenClawHotkeyWindow"
            };

            RegisterClass(ref wndClass);

            // Create message-only window (HWND_MESSAGE parent)
            _hwnd = CreateWindowEx(0, "OpenClawHotkeyWindow", "", 0, 0, 0, 0, 0,
                new IntPtr(-3), // HWND_MESSAGE
                IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero);

            // Message loop
            while (_running && GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Hotkey message loop error: {ex.Message}");
        }
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            OnHotkeyPressed();
        }
        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    public void Unregister()
    {
        if (!_registered) return;

        try
        {
            if (_hwnd != IntPtr.Zero)
            {
                UnregisterHotKey(_hwnd, HOTKEY_ID);
            }
            _registered = false;
            Logger.Info("Global hotkey unregistered");
        }
        catch (Exception ex)
        {
            Logger.Warn($"Hotkey unregistration error: {ex.Message}");
        }
    }

    internal void OnHotkeyPressed()
    {
        HotkeyPressed?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Unregister();

        _running = false;
        if (_hwnd != IntPtr.Zero)
        {
            PostMessage(_hwnd, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            DestroyWindow(_hwnd);
            _hwnd = IntPtr.Zero;
        }

        _messageThread?.Join(1000);
    }
}
