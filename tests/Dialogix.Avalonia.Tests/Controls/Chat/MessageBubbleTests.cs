using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Dialogix.Avalonia.Controls;

namespace Dialogix.Avalonia.Tests.Controls.Chat;

public class MessageBubbleTests
{
    [AvaloniaFact]
    public void Defaults_are_expected()
    {
        MessageBubble bubble = new MessageBubble();

        Assert.Null(bubble.Text);
        Assert.Null(bubble.Author);
        Assert.Null(bubble.Timestamp);
        Assert.False(bubble.IsOutgoing);
        Assert.Equal(0d, bubble.MinBubbleWidth);
        Assert.Equal(520d, bubble.MaxBubbleWidth);
        Assert.Equal(0, bubble.BoxShadow.Count);
    }

    [AvaloniaFact]
    public void IsOutgoing_toggles_outgoing_pseudo_class()
    {
        MessageBubble bubble = new MessageBubble();
        Assert.DoesNotContain(":outgoing", bubble.Classes);

        bubble.IsOutgoing = true;
        Assert.Contains(":outgoing", bubble.Classes);

        bubble.IsOutgoing = false;
        Assert.DoesNotContain(":outgoing", bubble.Classes);
    }

    [AvaloniaFact]
    public void Bubbles_are_rounded_by_default()
    {
        MessageBubble bubble = new MessageBubble();
        Show(bubble);

        Assert.Equal(new CornerRadius(12), bubble.CornerRadius);
    }

    [AvaloniaFact]
    public void Status_is_hidden_without_content()
    {
        MessageBubble bubble = new MessageBubble();
        Show(bubble);

        Assert.Null(bubble.Status);
        Assert.False(Part<ContentPresenter>(bubble, "PART_Status").IsVisible);
    }

    [AvaloniaFact]
    public void Status_sits_next_to_the_timestamp()
    {
        MessageBubble bubble = new MessageBubble { Text = "Hello", Timestamp = "12:00", Status = "Read" };
        Show(bubble);

        ContentPresenter status = Part<ContentPresenter>(bubble, "PART_Status");
        Assert.True(status.IsVisible);
        Assert.Equal("Read", status.Content);
        Assert.True(Part<TextBlock>(bubble, "PART_Timestamp").IsVisible);
    }

    [AvaloniaFact]
    public void Status_can_come_from_a_template()
    {
        MessageBubble bubble = new MessageBubble
        {
            Status = "Seen",
            StatusTemplate = new FuncDataTemplate<string>((text, _) => new TextBlock { Text = text })
        };
        Show(bubble);

        Assert.Single(bubble.GetVisualDescendants().OfType<TextBlock>(), x => x.Text == "Seen");
    }

    [AvaloniaFact]
    public void Template_parts_are_bound()
    {
        MessageBubble bubble = new MessageBubble { Text = "Hello", Author = "Guest", Timestamp = "12:00" };
        Show(bubble);

        Assert.Equal("Hello", Part<TextBlock>(bubble, "PART_Text").Text);
        Assert.Equal("Guest", Part<TextBlock>(bubble, "PART_Author").Text);
        Assert.Equal("12:00", Part<TextBlock>(bubble, "PART_Timestamp").Text);
    }

    [AvaloniaFact]
    public void Message_text_is_selectable()
    {
        MessageBubble bubble = new MessageBubble { Text = "Hello" };
        Show(bubble);

        Assert.IsType<SelectableTextBlock>(Part<TextBlock>(bubble, "PART_Text"));
    }

    [AvaloniaFact]
    public void Author_and_timestamp_are_hidden_when_missing()
    {
        MessageBubble bubble = new MessageBubble { Text = "Hello" };
        Show(bubble);

        Assert.False(Part<TextBlock>(bubble, "PART_Author").IsVisible);
        Assert.False(Part<TextBlock>(bubble, "PART_Timestamp").IsVisible);
    }

    [AvaloniaFact]
    public void Author_and_timestamp_are_visible_when_set()
    {
        MessageBubble bubble = new MessageBubble { Text = "Hello", Author = "Guest", Timestamp = "12:00" };
        Show(bubble);

        Assert.True(Part<TextBlock>(bubble, "PART_Author").IsVisible);
        Assert.True(Part<TextBlock>(bubble, "PART_Timestamp").IsVisible);
    }

    [AvaloniaFact]
    public void Outgoing_bubble_is_aligned_right()
    {
        MessageBubble bubble = new MessageBubble { Text = "Hello" };
        Show(bubble);

        Border part = Part<Border>(bubble, "PART_Bubble");
        Assert.Equal(HorizontalAlignment.Left, part.HorizontalAlignment);

        bubble.IsOutgoing = true;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(HorizontalAlignment.Right, part.HorizontalAlignment);
    }

    private static void Show(Control control)
    {
        Window window = new Window { Content = control, Width = 400, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static T Part<T>(Control control, string name) where T : Control =>
        control.GetVisualDescendants().OfType<T>().First(x => x.Name == name);
}