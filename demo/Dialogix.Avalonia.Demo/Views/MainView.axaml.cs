using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

using Dialogix.Avalonia.Demo.Views.Controls;

using FluentAvalonia.UI.Controls;

namespace Dialogix.Avalonia.Demo.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        Nav.SelectedItem = Nav.MenuItems[0];
        ShowPage("bubble");
    }

    private void OnThemeSwitchChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggle && Application.Current is { } app)
            app.RequestedThemeVariant = toggle.IsChecked == true ? ThemeVariant.Dark : ThemeVariant.Light;
    }

    private void OnNavigationSelectionChanged(object? sender, FANavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is FANavigationViewItem { Tag: string tag })
            ShowPage(tag);
    }

    private void ShowPage(string tag) =>
        PageHost.Content = tag switch
        {
            "bubble" => new MessageBubbleView(),
            "input" => new MessageInputView(),
            "chat" => new ChatViewView(),
            "profile" => new ProfilePage(),
            _ => null
        };
}