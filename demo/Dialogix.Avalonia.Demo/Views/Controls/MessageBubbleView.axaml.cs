using Avalonia;
using Avalonia.Controls;

namespace Dialogix.Avalonia.Demo.Views.Controls;

public partial class MessageBubbleView : UserControl
{
    public MessageBubbleView()
    {
        InitializeComponent();
        SizeChanged += (_, e) => PageContent.Margin = new Thickness(e.NewSize.Width < 1000 ? 16 : 24);
    }
}