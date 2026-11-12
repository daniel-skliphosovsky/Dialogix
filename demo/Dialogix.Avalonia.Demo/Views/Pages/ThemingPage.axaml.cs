using System.Collections.ObjectModel;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

using Dialogix.Avalonia.Controls;
using Dialogix.Avalonia.Demo.Infrastructure;

namespace Dialogix.Avalonia.Demo.Views.Pages;

public partial class ThemingPage : UserControl
{
    public ThemingPage()
    {
        OceanSendCommand = new RelayCommand(parameter =>
        {
            if (parameter is not string text || string.IsNullOrWhiteSpace(text))
                return;

            OceanMessages.Add(new MessageBubble
            {
                Author = "You",
                Text = text,
                Timestamp = "Now",
                IsOutgoing = true
            });
            OceanChat.ScrollToEnd();
        });

        MatrixSendCommand = new RelayCommand(parameter =>
        {
            if (parameter is not string text || string.IsNullOrWhiteSpace(text))
                return;

            MatrixMessages.Add(new MessageBubble
            {
                Author = "You",
                Text = text,
                Timestamp = "Now",
                IsOutgoing = true
            });
            MatrixChat.ScrollToEnd();
        });

        PerElementSendCommand = new RelayCommand(parameter =>
        {
            if (parameter is not string text || string.IsNullOrWhiteSpace(text))
                return;

            PerElementMessages.Children.Add(new MessageBubble
            {
                Author = "You",
                Text = text,
                Timestamp = "Now",
                IsOutgoing = true,
                Background = new SolidColorBrush(Color.Parse("#FF065F46")),
                Foreground = new SolidColorBrush(Color.Parse("#FFECFDF5")),
                CornerRadius = new CornerRadius(18)
            });
        });

        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<MessageBubble> OceanMessages { get; } =
    [
        new MessageBubble { Author = "Nina", Text = "One palette, every control.", Timestamp = "12:38" },
        new MessageBubble { Author = "You", Text = "No /template/ edits.", Timestamp = "12:39", IsOutgoing = true },
        new MessageBubble { Author = "Nina", Text = "Send a message to try it.", Timestamp = "12:41" }
    ];

    public ObservableCollection<MessageBubble> MatrixMessages { get; } =
    [
        new MessageBubble { Author = "Guest", Text = "Connection established.", Timestamp = "12:38" },
        new MessageBubble { Author = "You", Text = "Follow the white rabbit.", Timestamp = "12:39", IsOutgoing = true },
        new MessageBubble { Author = "Guest", Text = "Type a message to try it.", Timestamp = "12:41" }
    ];

    public ICommand OceanSendCommand { get; }

    public ICommand MatrixSendCommand { get; }

    public ICommand PerElementSendCommand { get; }
}