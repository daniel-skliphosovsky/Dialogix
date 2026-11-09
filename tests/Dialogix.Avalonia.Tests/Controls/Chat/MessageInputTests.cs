using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Dialogix.Avalonia.Controls;


namespace Dialogix.Avalonia.Tests.Controls.Chat;

public class MessageInputTests
{
    [AvaloniaFact]
    public void Defaults_are_expected()
    {
        MessageInput input = new MessageInput();

        Assert.Null(input.Text);
        Assert.Equal("Message", input.Watermark);
        Assert.Equal(5, input.MaxLines);
        Assert.Equal(0, input.MaxLength);
        Assert.Null(input.SendCommand);
        Assert.Null(input.LeadingContent);
        Assert.Null(input.LeadingContentTemplate);
        Assert.Null(input.ActionContent);
        Assert.Null(input.ActionContentTemplate);
    }

    [AvaloniaFact]
    public void Template_parts_are_bound()
    {
        MessageInput input = new MessageInput { Text = "Hello", Watermark = "Type here" };
        Show(input);

        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        Assert.Equal("Hello", textBox.Text);
        Assert.Equal("Type here", textBox.PlaceholderText);
    }

    [AvaloniaFact]
    public void Enter_sends_text_and_clears_text_box()
    {
        Recorder recorder = new Recorder();
        MessageInput input = new MessageInput { SendCommand = recorder };
        Show(input);

        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        textBox.Text = "Hello";
        Dispatcher.UIThread.RunJobs();

        Assert.True(Press(input, Key.Enter, KeyModifiers.None).Handled);

        Assert.Equal(["Hello"], recorder.Sent);
        Assert.Equal(string.Empty, input.Text);
        Assert.Equal(string.Empty, textBox.Text);
    }

    [AvaloniaFact]
    public void Shift_enter_does_not_send_and_keeps_text()
    {
        Recorder recorder = new Recorder();
        MessageInput input = new MessageInput { SendCommand = recorder };
        Show(input);

        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        textBox.Text = "Hello";
        Dispatcher.UIThread.RunJobs();

        Press(input, Key.Enter, KeyModifiers.Shift);

        Assert.Empty(recorder.Sent);
        Assert.Contains("\n", textBox.Text);
        Assert.Contains("Hello", textBox.Text);
    }

    [AvaloniaFact]
    public void Blank_text_is_not_sent_and_is_kept()
    {
        Recorder recorder = new Recorder();
        MessageInput input = new MessageInput { SendCommand = recorder };
        Show(input);

        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        textBox.Text = "   ";
        Dispatcher.UIThread.RunJobs();

        Press(input, Key.Enter, KeyModifiers.None);

        Assert.Empty(recorder.Sent);
        Assert.Equal("   ", input.Text);
    }

    [AvaloniaFact]
    public void Text_is_kept_when_command_cannot_execute()
    {
        Recorder recorder = new Recorder { CanExecuteResult = false };
        MessageInput input = new MessageInput { Text = "Hello", SendCommand = recorder };
        Show(input);

        Press(input, Key.Enter, KeyModifiers.None);

        Assert.Empty(recorder.Sent);
        Assert.Equal("Hello", input.Text);
    }

    [AvaloniaFact]
    public void Send_button_is_disabled_until_text_is_typed()
    {
        MessageInput input = new MessageInput();
        Show(input);

        Button button = Part<Button>(input, "PART_SendButton");
        Assert.False(button.IsEnabled);

        Part<TextBox>(input, "PART_TextBox").Text = "Hi";
        Dispatcher.UIThread.RunJobs();

        Assert.True(button.IsEnabled);
    }

    [AvaloniaFact]
    public void Send_button_sends_text_and_clears_text_box()
    {
        Recorder recorder = new Recorder();
        MessageInput input = new MessageInput { SendCommand = recorder };
        Show(input);

        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        textBox.Text = "Hello";
        Dispatcher.UIThread.RunJobs();

        Part<Button>(input, "PART_SendButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(["Hello"], recorder.Sent);
        Assert.Equal(string.Empty, textBox.Text);
    }

    [AvaloniaFact]
    public void Focus_moves_pseudo_class_on_the_input()
    {
        MessageInput input = new MessageInput();
        Show(input);

        Part<TextBox>(input, "PART_TextBox").Focus();
        Dispatcher.UIThread.RunJobs();

        Assert.Contains(":focused", input.Classes);
    }

    [AvaloniaFact]
    public void Focused_text_box_has_no_inner_frame()
    {
        MessageInput input = new MessageInput();
        Show(input);

        Part<TextBox>(input, "PART_TextBox").Focus();
        Dispatcher.UIThread.RunJobs();

        Border frame = input.GetVisualDescendants().OfType<Border>().First(x => x.Name == "PART_BorderElement");
        Assert.Equal(new Thickness(0), frame.BorderThickness);
        Assert.Equal(Colors.Transparent, Assert.IsAssignableFrom<ISolidColorBrush>(frame.Background).Color);
    }

    [AvaloniaFact]
    public void Focused_text_box_keeps_the_input_foreground()
    {
        MessageInput input = new MessageInput { Foreground = new SolidColorBrush(Colors.White) };
        Show(input);

        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        textBox.Focus();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(Colors.White, Assert.IsAssignableFrom<ISolidColorBrush>(textBox.Foreground).Color);
    }

    [AvaloniaFact]
    public void Caret_uses_the_input_foreground()
    {
        MessageInput input = new MessageInput { Foreground = new SolidColorBrush(Colors.Red) };
        Show(input);

        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        textBox.Focus();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(Colors.Red, Assert.IsAssignableFrom<ISolidColorBrush>(textBox.CaretBrush).Color);
    }

    [AvaloniaFact]
    public void Pointer_over_does_not_draw_an_inner_frame()
    {
        MessageInput input = new MessageInput();
        Show(input);

        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        ((IPseudoClasses)textBox.Classes).Set(":pointerover", true);
        Dispatcher.UIThread.RunJobs();

        Border frame = input.GetVisualDescendants().OfType<Border>().First(x => x.Name == "PART_BorderElement");
        Assert.Equal(new Thickness(0), frame.BorderThickness);
    }

    [AvaloniaFact]
    public void Grows_up_to_max_lines_then_stops()
    {
        MessageInput input = new MessageInput { MaxLines = 3 };
        Window window = new() { Width = 320, Height = 500, Content = input };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        textBox.Text = "a\nb\nc";
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        double threeLines = textBox.Bounds.Height;

        textBox.Text = string.Join('\n', Enumerable.Range(0, 12).Select(i => $"line {i}"));
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        Assert.True(double.IsFinite(textBox.MaxHeight));
        Assert.Equal(textBox.MaxHeight, textBox.Bounds.Height, 1);
        Assert.Equal(threeLines, textBox.Bounds.Height, 1);
    }

    [AvaloniaFact]
    public void Empty_input_stays_one_line()
    {
        MessageInput tall = new MessageInput { MaxLines = 5 };
        MessageInput single = new MessageInput { MaxLines = 1 };
        StackPanel panel = new() { Spacing = 8, Children = { tall, single } };
        Window window = new() { Width = 320, Height = 400, Content = panel };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        Assert.Equal(Part<TextBox>(single, "PART_TextBox").Bounds.Height,
            Part<TextBox>(tall, "PART_TextBox").Bounds.Height, 1);

        Part<TextBox>(tall, "PART_TextBox").Text = "a\nb";
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        Assert.True(Part<TextBox>(tall, "PART_TextBox").Bounds.Height >
            Part<TextBox>(single, "PART_TextBox").Bounds.Height);
    }

    [AvaloniaFact]
    public void Counter_tracks_length_and_flags_overflow()
    {
        MessageInput input = new MessageInput { MaxLength = 5, Text = "abc" };
        Show(input);

        TextBlock counter = Part<TextBlock>(input, "PART_Counter");

        Assert.True(counter.IsVisible);
        Assert.Equal("3/5", counter.Text);
        Assert.False(input.IsTextTooLong);
        Assert.True(Part<Button>(input, "PART_SendButton").IsEnabled);

        Part<TextBox>(input, "PART_TextBox").Text = "abcdef";
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("6/5", counter.Text);
        Assert.True(input.IsTextTooLong);
        Assert.Equal(6, input.CharacterCount);
        Assert.False(Part<Button>(input, "PART_SendButton").IsEnabled);
    }

    [AvaloniaFact]
    public void Too_long_text_is_not_sent()
    {
        Recorder recorder = new Recorder();
        MessageInput input = new MessageInput { MaxLength = 3, Text = "abcd", SendCommand = recorder };
        Show(input);

        Press(input, Key.Enter, KeyModifiers.None);

        Assert.Empty(recorder.Sent);
        Assert.Equal("abcd", input.Text);
    }

    [AvaloniaFact]
    public void Counter_is_hidden_without_a_limit()
    {
        MessageInput input = new MessageInput { Text = "Hello" };
        Show(input);

        Assert.False(Part<TextBlock>(input, "PART_Counter").IsVisible);
    }

    [AvaloniaFact]
    public void Placeholder_stays_muted_on_pointer_over_and_focus()
    {
        MessageInput input = new MessageInput();
        Show(input);

        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        TextBlock placeholder = input.GetVisualDescendants().OfType<TextBlock>().First(x => x.Name == "PART_Placeholder");

        Assert.True(input.TryFindResource("DialogixMutedForegroundBrush", out object? muted));
        Color expected = Assert.IsAssignableFrom<ISolidColorBrush>(muted).Color;
        Assert.Equal(expected, Assert.IsAssignableFrom<ISolidColorBrush>(placeholder.Foreground).Color);

        ((IPseudoClasses)textBox.Classes).Set(":pointerover", true);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(expected, Assert.IsAssignableFrom<ISolidColorBrush>(placeholder.Foreground).Color);

        textBox.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(expected, Assert.IsAssignableFrom<ISolidColorBrush>(placeholder.Foreground).Color);
    }

    [AvaloniaFact]
    public void Clicking_the_composer_focuses_the_text_box()
    {
        MessageInput input = new MessageInput();
        Window window = new() { Content = input, Width = 400, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        Assert.False(textBox.IsFocused);

        Border frame = Part<Border>(input, "PART_Frame");
        Point point = frame.TranslatePoint(new Point(2, frame.Bounds.Height / 2), window)!.Value;

        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.True(textBox.IsFocused);
    }

    [AvaloniaFact]
    public void Escape_removes_focus()
    {
        MessageInput input = new MessageInput();
        Window window = new() { Content = input, Width = 400, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        textBox.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.True(textBox.IsFocused);

        KeyEventArgs args = Press(input, Key.Escape, KeyModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(args.Handled);
        Assert.False(textBox.IsFocused);
    }

    [AvaloniaFact]
    public void Escape_removes_focus_from_any_part_of_the_composer()
    {
        MessageInput input = new MessageInput { Text = "Hello" };
        Window window = new() { Content = input, Width = 400, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Button button = Part<Button>(input, "PART_SendButton");
        button.Focus();
        Dispatcher.UIThread.RunJobs();
        Assert.True(button.IsFocused);

        window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
        Dispatcher.UIThread.RunJobs();

        Assert.False(button.IsFocused);
    }

    [AvaloniaFact]
    public void Leading_and_action_content_are_hosted()
    {
        TextBlock leading = new() { Text = "Attach" };
        Button action = new() { Content = "More" };
        MessageInput input = new() { LeadingContent = leading, ActionContent = action };
        Show(input);

        ContentPresenter leadingHost = Part<ContentPresenter>(input, "PART_Leading");
        ContentPresenter actionHost = Part<ContentPresenter>(input, "PART_Action");

        Assert.Same(leading, leadingHost.Content);
        Assert.Same(action, actionHost.Content);
        Assert.True(leadingHost.IsVisible);
        Assert.True(actionHost.IsVisible);
    }

    [AvaloniaFact]
    public void Slot_content_can_come_from_templates()
    {
        MessageInput input = new()
        {
            LeadingContent = "pin",
            LeadingContentTemplate = new FuncDataTemplate<string>((text, _) => new TextBlock { Text = text }),
            ActionContent = "extra",
            ActionContentTemplate = new FuncDataTemplate<string>((text, _) => new TextBlock { Text = text })
        };
        Show(input);

        Assert.Single(input.GetVisualDescendants().OfType<TextBlock>(), x => x.Text == "pin");
        Assert.Single(input.GetVisualDescendants().OfType<TextBlock>(), x => x.Text == "extra");
    }

    [AvaloniaFact]
    public void Send_icon_is_themed_and_replaceable()
    {
        MessageInput input = new MessageInput();
        Show(input);

        IconPresenter icon = input.GetVisualDescendants().OfType<IconPresenter>().First();
        Assert.NotNull(input.SendIcon);
        Assert.Same(input.SendIcon, icon.Source);

        Geometry custom = Geometry.Parse("M0 0 L1 1");
        input.SendIcon = custom;
        Dispatcher.UIThread.RunJobs();

        Assert.Same(custom, input.GetVisualDescendants().OfType<IconPresenter>().First().Source);
    }

    private static KeyEventArgs Press(MessageInput input, Key key, KeyModifiers modifiers)
    {
        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        KeyEventArgs args = new()
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = key,
            KeyModifiers = modifiers,
            Source = textBox
        };

        textBox.RaiseEvent(args);
        return args;
    }

    private static void Show(Control control)
    {
        Window window = new Window { Content = control, Width = 400, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static T Part<T>(Control control, string name) where T : Control =>
        control.GetVisualDescendants().OfType<T>().First(x => x.Name == name);

    private sealed class Recorder : ICommand
    {
        public List<string> Sent { get; } = [];
        public bool CanExecuteResult { get; init; } = true;

        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => CanExecuteResult;

        public void Execute(object? parameter) => Sent.Add((string)parameter!);
    }
}