using Avalonia;
using Avalonia.Controls;

namespace Dialogix.Avalonia.Demo.Views.Pages;

public partial class BubblePage : UserControl
{
    public BubblePage()
    {
        InitializeComponent();
        SizeChanged += (_, e) => PageContent.Margin = new Thickness(e.NewSize.Width < 1000 ? 16 : 24);
    }
}