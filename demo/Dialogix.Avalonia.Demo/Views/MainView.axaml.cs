using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Dialogix.Avalonia.Demo.Views.Controls;

namespace Dialogix.Avalonia.Demo.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        NavigationList.SelectedIndex = 0;
    }

    private void OnThemeSwitchChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggle && Application.Current is { } app)
            app.RequestedThemeVariant = toggle.IsChecked == true ? ThemeVariant.Dark : ThemeVariant.Light;
    }

    private void OnNavigationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        PageHost.Content = NavigationList.SelectedIndex switch
        {
            0 => new MessageBubbleView(),
            _ => null
        };
    }
}