using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Dialogix.Avalonia.Controls;

namespace Dialogix.Avalonia.Tests.Controls;

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
        ChatView chat = new ChatView();
        chat.Resources["DialogixChatBackgroundBrush"] = Brushes.Green;
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

    private static void Show(Control control)
    {
        Window window = new() { Content = control, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static T Part<T>(Control control, string name) where T : Control =>
        control.GetVisualDescendants().OfType<T>().First(x => x.Name == name);
}