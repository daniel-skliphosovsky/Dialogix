using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

using Dialogix.Avalonia.Controls;
using Dialogix.Avalonia.Demo.Infrastructure;

namespace Dialogix.Avalonia.Demo.Views.Pages;

public partial class InputPage : UserControl
{
    public InputPage()
    {
        InitializeComponent();

        Wire(InputDefault, SentDefault);
        Wire(InputViolet, SentViolet);
        Wire(InputMono, SentMono);
        Wire(InputMatrix, SentMatrix);
        Wire(InputMulti, SentMulti);
        Wire(InputLimit, SentLimit);
        Wire(InputIcon, SentIcon);
        Wire(InputImageIcon, SentImageIcon);

        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e) => ApplyWidth(e.NewSize.Width);

    private void ApplyWidth(double width)
    {
        if (PageContent is null)
            return;

        bool narrow = width < 1000;
        double margin = narrow ? 16 : 24;
        double available = Math.Max(240, width - (margin * 2) - 1);

        PageContent.Margin = new Thickness(margin);
        PageTitle.FontSize = narrow ? 22 : 26;
        Cards.ItemWidth = narrow ? available : 326;

        foreach (Border card in Cards.Children.OfType<Border>())
        {
            card.Margin = narrow ? new Thickness(0, 0, 0, 12) : new Thickness(0, 0, 10, 10);
            card.VerticalAlignment = VerticalAlignment.Top;
        }
    }

    private static void Wire(MessageInput input, TextBlock result) =>
        input.SendCommand = new RelayCommand(parameter => result.Text = $"Sent: {parameter}");
}