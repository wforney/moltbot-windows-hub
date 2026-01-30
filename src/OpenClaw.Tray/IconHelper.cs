using System.Drawing;

namespace OpenClawTray;

/// <summary>
/// Shared icon helper for creating the lobster icon used throughout the app.
/// </summary>
public static class IconHelper
{
    private static Icon? _cachedLobsterIcon;

    /// <summary>
    /// Gets the lobster icon for use in forms and windows.
    /// </summary>
    public static Icon GetLobsterIcon()
    {
        if (_cachedLobsterIcon != null)
            return _cachedLobsterIcon;

        var bitmap = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Transparent);
            DrawPixelLobster(g);
        }

        var hIcon = bitmap.GetHicon();
        _cachedLobsterIcon = Icon.FromHandle(hIcon);
        return _cachedLobsterIcon;
    }

    private static void DrawPixelLobster(Graphics g)
    {
        // Pixel lobster from SVG - 16x16 pixel art
        var outline = Color.FromArgb(58, 10, 13);      // #3a0a0d - dark outline
        var body = Color.FromArgb(255, 79, 64);        // #ff4f40 - red body
        var claw = Color.FromArgb(255, 119, 95);       // #ff775f - lighter claws
        var eyeDark = Color.FromArgb(8, 16, 22);       // #081016 - pupils
        var eyeLight = Color.FromArgb(245, 251, 255); // #f5fbff - eye whites

        // Outline (dark border)
        var outlinePixels = new[] {
            (1,5), (1,6), (1,7),
            (2,4), (2,8),
            (3,3), (3,9),
            (4,2), (4,10),
            (5,2), (6,2), (7,2), (8,2), (9,2), (10,2),
            (11,2), (12,3), (12,9),
            (13,4), (13,8),
            (14,5), (14,6), (14,7),
            (5,11), (6,11), (7,11), (8,11), (9,11), (10,11),
            (4,12), (11,12),
            (3,13), (12,13),
            (5,14), (6,14), (7,14), (8,14), (9,14), (10,14)
        };
        foreach (var (x, y) in outlinePixels)
            SetPixel(g, x, y, outline);

        // Body (red)
        var bodyPixels = new[] {
            (5,3), (6,3), (7,3), (8,3), (9,3), (10,3),
            (4,4), (5,4), (7,4), (8,4), (10,4), (11,4),
            (3,5), (4,5), (5,5), (7,5), (8,5), (10,5), (11,5), (12,5),
            (3,6), (4,6), (5,6), (6,6), (7,6), (8,6), (9,6), (10,6), (11,6), (12,6),
            (3,7), (4,7), (5,7), (6,7), (7,7), (8,7), (9,7), (10,7), (11,7), (12,7),
            (4,8), (5,8), (6,8), (7,8), (8,8), (9,8), (10,8), (11,8),
            (5,9), (6,9), (7,9), (8,9), (9,9), (10,9),
            (5,12), (6,12), (7,12), (8,12), (9,12), (10,12),
            (6,13), (7,13), (8,13), (9,13)
        };
        foreach (var (x, y) in bodyPixels)
            SetPixel(g, x, y, body);

        // Claws (lighter red)
        var clawPixels = new[] {
            (1,6), (2,5), (2,6), (2,7),
            (13,5), (13,6), (13,7), (14,6)
        };
        foreach (var (x, y) in clawPixels)
            SetPixel(g, x, y, claw);

        // Eyes
        SetPixel(g, 6, 4, eyeLight);
        SetPixel(g, 9, 4, eyeLight);
        SetPixel(g, 6, 5, eyeDark);
        SetPixel(g, 9, 5, eyeDark);
    }

    private static void SetPixel(Graphics g, int x, int y, Color c)
    {
        using var brush = new SolidBrush(c);
        g.FillRectangle(brush, x, y, 1, 1);
    }
}

