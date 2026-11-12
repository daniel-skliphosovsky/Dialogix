using System.Collections.ObjectModel;
using System.Windows.Input;

using Avalonia.Controls;

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

    public ICommand OceanSendCommand { get; }
}