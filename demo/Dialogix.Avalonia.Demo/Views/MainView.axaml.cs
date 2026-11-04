using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

using Dialogix.Avalonia.Demo.Views.Controls;

using FluentAvalonia.UI.Controls;

namespace Dialogix.Avalonia.Demo.Views;

public partial class MainView : UserControl
{
    private bool _top;

    public MainView()
    {
        InitializeComponent();

        Nav.SelectedItem = Nav.MenuItems[0];
        ShowPage("bubble");

        SizeChanged += OnSizeChanged;
        Nav.PropertyChanged += OnNavPropertyChanged;
    }

    private void OnNavPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == FANavigationView.IsPaneOpenProperty || e.Property == FANavigationView.DisplayModeProperty)
            ApplyNavChrome();
    }

    private void ApplyNavChrome()
    {
        bool open = Nav.IsPaneOpen && !_top;

        ThemeSwitch.Content = open ? "Dark theme" : string.Empty;
        ThemeSwitch.Margin = open ? new Thickness(12, 8, 12, 16) : new Thickness(12, 8, 0, 16);

        BubbleItem.IconSource = _top ? null : this.FindResource("BubbleIcon") as FAIconSource;
        InputItem.IconSource = _top ? null : this.FindResource("InputIcon") as FAIconSource;
        ChatItem.IconSource = _top ? null : this.FindResource("ChatIcon") as FAIconSource;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        bool compact = e.NewSize.Width < 720;
        _top = compact;

        Nav.PaneDisplayMode = compact
            ? FANavigationViewPaneDisplayMode.Top
            : FANavigationViewPaneDisplayMode.Left;

        Nav.PaneTitle = compact ? string.Empty : "Dialogix";

        if (!compact)
            Nav.IsPaneOpen = true;

        BubbleItem.Content = compact ? "Bubbles" : "Message Bubble";
        InputItem.Content = compact ? "Input" : "Message Input";
        ChatItem.Content = compact ? "Chat" : "Chat View";
        ThemeSwitch.IsVisible = !compact;
        CompactThemePanel.IsVisible = compact;

        ApplyNavChrome();
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
            _ => null
        };
}