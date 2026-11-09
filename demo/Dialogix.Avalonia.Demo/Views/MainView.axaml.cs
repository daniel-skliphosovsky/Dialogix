using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

using Dialogix.Avalonia.Demo.Infrastructure;
using Dialogix.Avalonia.Demo.Views.Pages;

using FluentAvalonia.UI.Controls;

namespace Dialogix.Avalonia.Demo.Views;

public partial class MainView : UserControl
{
    private readonly Dictionary<string, Control> _pages = [];

    public MainView()
    {
        InitializeComponent();

        Nav.SelectedItem = Nav.MenuItems[0];
        ShowPage("bubble");
    }

    private void OnThemeSwitchChanged(object? sender, RoutedEventArgs e) => ApplyTheme();

    private void OnHackerSwitchChanged(object? sender, RoutedEventArgs e) => ApplyTheme();

    private void ApplyTheme()
    {
        if (Application.Current is not { } app)
            return;

        bool hacker = HackerSwitch.IsChecked == true;

        app.RequestedThemeVariant = hacker
            ? DemoTheme.Hacker
            : ThemeSwitch.IsChecked == true
                ? ThemeVariant.Dark
                : ThemeVariant.Light;

        Nav.FontFamily = hacker ? DemoTheme.HackerFont : DemoTheme.DefaultFont;
    }

    private void OnNavigationSelectionChanged(object? sender, FANavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is FANavigationViewItem { Tag: string tag })
            ShowPage(tag);
    }

    private void ShowPage(string tag)
    {
        if (!_pages.TryGetValue(tag, out Control? page))
        {
            page = CreatePage(tag);

            if (page is null)
                return;

            _pages[tag] = page;
        }

        PageHost.Content = page;
    }

    private static Control? CreatePage(string tag) =>
        tag switch
        {
            "bubble" => new BubblePage(),
            "input" => new InputPage(),
            "chat" => new ChatPage(),
            "conversations" => new ConversationListPage(),
            "messenger" => new MessengerPage(),
            "theming" => new ThemingPage(),
            "profile" => new ProfilePage(),
            _ => null
        };
}