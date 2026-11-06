using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Dialogix.Avalonia.Controls;

namespace Dialogix.Avalonia.Tests.Themes;

public class ThemingTests
{
    [AvaloniaFact]
    public void App_styles_override_the_default_control_theme()
    {
        Style style = new(selector => selector.OfType<ChatView>());
        style.Setters.Add(new Setter(TemplatedControl.BackgroundProperty, Brushes.Red));

        Application.Current!.Styles.Add(style);
        try
        {
            ChatView chat = new ChatView();
            Show(chat);

            Assert.Equal(Brushes.Red, Part<Border>(chat, "PART_Frame").Background);
        }
        finally
        {
            Application.Current.Styles.Remove(style);
        }
    }

    [AvaloniaFact]
    public void Scoped_brushes_override_the_library_defaults()
    {
        ChatView chat = new ChatView { Resources = { ["DialogixChatBackgroundBrush"] = Brushes.Green } };
        Show(chat);

        Assert.Equal(Brushes.Green, Part<Border>(chat, "PART_Frame").Background);
    }

    [AvaloniaFact]
    public void Scoped_styles_restyle_bubbles_inside_a_chat_view()
    {
        MessageBubble bubble = new() { Text = "Hi" };
        ChatView chat = new() { Messages = new MessageBubble[] { bubble }, Classes = { "custom" } };

        Style style = new(selector => selector
            .OfType<ChatView>().Class("custom")
            .Descendant().OfType<MessageBubble>());
        style.Setters.Add(new Setter(TemplatedControl.BackgroundProperty, Brushes.Orange));

        chat.Styles.Add(style);
        Show(chat);

        Assert.Equal(Brushes.Orange, Part<Border>(bubble, "PART_Bubble").Background);
    }

    [AvaloniaFact]
    public void Dialogix_text_box_theme_paints_with_the_palette()
    {
        TextBox textBox = new TextBox { PlaceholderText = "Type here" };
        Show(textBox);

        textBox.Theme = Assert.IsAssignableFrom<ControlTheme>(textBox.FindResource("DialogixTextBoxTheme"));
        Dispatcher.UIThread.RunJobs();

        textBox.Focus();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(BrushColor(textBox, "DialogixAccentBrush"), Assert.IsAssignableFrom<ISolidColorBrush>(textBox.SelectionBrush).Color);
        Assert.Equal(BrushColor(textBox, "DialogixAccentForegroundBrush"), Assert.IsAssignableFrom<ISolidColorBrush>(textBox.SelectionForegroundBrush).Color);
        Assert.Equal(BrushColor(textBox, "DialogixMutedForegroundBrush"), Assert.IsAssignableFrom<ISolidColorBrush>(textBox.PlaceholderForeground).Color);
        Assert.Equal(Assert.IsAssignableFrom<ISolidColorBrush>(textBox.Foreground).Color, Assert.IsAssignableFrom<ISolidColorBrush>(textBox.CaretBrush).Color);
        Assert.Equal(BrushColor(textBox, "DialogixPrimaryBrush"), Assert.IsAssignableFrom<ISolidColorBrush>(Part<Border>(textBox, "PART_BorderElement").BorderBrush).Color);
    }

    [AvaloniaFact]
    public void Scoped_palette_reaches_the_nested_text_box()
    {
        MessageInput input = new MessageInput
        {
            Resources =
            {
                ["DialogixPrimaryBrush"] = Brushes.Red,
                ["DialogixMutedForegroundBrush"] = Brushes.Green
            }
        };
        Show(input);

        Part<TextBox>(input, "PART_TextBox").Focus();
        Dispatcher.UIThread.RunJobs();

        Border frame = input.GetVisualDescendants().OfType<Border>().First(x => x.Name == "PART_BorderElement");
        Assert.Equal(Colors.Red, Assert.IsAssignableFrom<ISolidColorBrush>(frame.BorderBrush).Color);
        Assert.Equal(Colors.Green, Assert.IsAssignableFrom<ISolidColorBrush>(Part<TextBlock>(input, "PART_Placeholder").Foreground).Color);
    }

    [AvaloniaFact]
    public void Chat_scroll_bar_uses_the_palette()
    {
        ChatView chat = new ChatView
        {
            Messages = Enumerable.Range(0, 40).Select(i => new MessageBubble { Text = $"Message {i}" }).ToArray()
        };
        Show(chat);
        Dispatcher.UIThread.RunJobs();

        ScrollBar bar = chat.GetVisualDescendants().OfType<ScrollBar>().First();
        Assert.Equal(12d, bar.MinWidth);
        Assert.Equal(BrushColor(chat, "DialogixMutedForegroundBrush"), Assert.IsAssignableFrom<ISolidColorBrush>(bar.Foreground).Color);
    }

    private static Color BrushColor(Control control, string key)
    {
        Assert.True(control.TryFindResource(key, out object? value));
        return Assert.IsAssignableFrom<ISolidColorBrush>(value).Color;
    }

    private static void Show(Control control)
    {
        Window window = new() { Content = control, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static T Part<T>(Control control, string name) where T : Control =>
        control.GetVisualDescendants().OfType<T>().First(x => x.Name == name);
}