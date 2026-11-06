using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Dialogix.Avalonia.Controls;

namespace Dialogix.Avalonia.Tests.Controls.Chat;

public class ChatHeaderTests
{
    [AvaloniaFact]
    public void Defaults_are_expected()
    {
        ChatHeader header = new ChatHeader();

        Assert.Null(header.Title);
        Assert.Null(header.Status);
        Assert.Equal(0, header.BoxShadow.Count);
    }

    [AvaloniaFact]
    public void Template_parts_are_bound()
    {
        ChatHeader header = new ChatHeader { Title = "Design team", Status = "12 members, 3 online" };
        Show(header);

        Assert.Equal("Design team", Part<TextBlock>(header, "PART_Title").Text);
        Assert.Equal("12 members, 3 online", Part<TextBlock>(header, "PART_Status").Text);
    }

    [AvaloniaFact]
    public void Title_and_status_are_hidden_when_missing()
    {
        ChatHeader header = new ChatHeader();
        Show(header);

        Assert.False(Part<TextBlock>(header, "PART_Title").IsVisible);
        Assert.False(Part<TextBlock>(header, "PART_Status").IsVisible);
    }

    [AvaloniaFact]
    public void Title_and_status_are_visible_when_set()
    {
        ChatHeader header = new ChatHeader { Title = "Nina", Status = "Online" };
        Show(header);

        Assert.True(Part<TextBlock>(header, "PART_Title").IsVisible);
        Assert.True(Part<TextBlock>(header, "PART_Status").IsVisible);
    }

    [AvaloniaFact]
    public void Min_width_stretches_the_island()
    {
        ChatHeader header = new ChatHeader
        {
            Title = "Nina",
            MinWidth = 220,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Show(header);

        Assert.True(Part<Border>(header, "PART_Island").Bounds.Width >= 220);
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