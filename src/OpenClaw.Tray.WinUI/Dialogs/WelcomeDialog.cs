using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using WinUIEx;

namespace OpenClawTray.Dialogs;

/// <summary>
/// First-run welcome dialog for new users.
/// </summary>
public sealed class WelcomeDialog : WindowEx
{
    private readonly TaskCompletionSource<ContentDialogResult> _tcs = new();
    private ContentDialogResult _result = ContentDialogResult.None;

    public WelcomeDialog()
    {
        Title = "Welcome to OpenClaw";
        this.SetWindowSize(480, 440);
        this.CenterOnScreen();
        this.SetIcon("Assets\\moltbot.ico");
        
        // Apply Mica backdrop for modern Windows 11 look
        SystemBackdrop = new MicaBackdrop();
        
        // Build UI directly in the window (no ContentDialog needed)
        var root = new Grid
        {
            Padding = new Thickness(32),
            RowSpacing = 16
        };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Lobster header
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        headerPanel.Children.Add(new TextBlock
        {
            Text = "🦞",
            FontSize = 48
        });
        headerPanel.Children.Add(new TextBlock
        {
            Text = "Welcome to OpenClaw!",
            Style = (Style)Application.Current.Resources["TitleTextBlockStyle"],
            VerticalAlignment = VerticalAlignment.Center
        });
        Grid.SetRow(headerPanel, 0);
        root.Children.Add(headerPanel);

        // Content
        var content = new StackPanel { Spacing = 16 };
        
        content.Children.Add(new TextBlock
        {
            Text = "OpenClaw Tray is your Windows companion for OpenClaw, the AI-powered personal assistant.",
            TextWrapping = TextWrapping.Wrap
        });

        var gettingStarted = new StackPanel { Spacing = 8 };
        gettingStarted.Children.Add(new TextBlock
        {
            Text = "To get started, you'll need:",
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });

        var bulletList = new StackPanel { Spacing = 4, Margin = new Thickness(16, 0, 0, 0) };
        bulletList.Children.Add(new TextBlock { Text = "• A running OpenClaw gateway" });
        bulletList.Children.Add(new TextBlock { Text = "• Your API token from the dashboard" });
        gettingStarted.Children.Add(bulletList);
        content.Children.Add(gettingStarted);

        var docsButton = new HyperlinkButton
        {
            Content = "📚 View Documentation",
            NavigateUri = new Uri("https://docs.molt.bot/web/dashboard")
        };
        content.Children.Add(docsButton);

        Grid.SetRow(content, 1);
        root.Children.Add(content);

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };

        var laterButton = new Button { Content = "Later" };
        laterButton.Click += (s, e) =>
        {
            _result = ContentDialogResult.None;
            Close();
        };
        buttonPanel.Children.Add(laterButton);

        var settingsButton = new Button
        {
            Content = "Open Settings",
            Style = (Style)Application.Current.Resources["AccentButtonStyle"]
        };
        settingsButton.Click += (s, e) =>
        {
            _result = ContentDialogResult.Primary;
            Close();
        };
        buttonPanel.Children.Add(settingsButton);

        Grid.SetRow(buttonPanel, 2);
        root.Children.Add(buttonPanel);

        Content = root;
        
        Closed += (s, e) => _tcs.TrySetResult(_result);
    }

    public new Task<ContentDialogResult> ShowAsync()
    {
        Activate();
        return _tcs.Task;
    }
}
