using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Dialogix.Avalonia.Controls;

using ShapePath = Avalonia.Controls.Shapes.Path;

namespace Dialogix.Avalonia.Tests.Controls;

public class MessageInputTests
{
    [AvaloniaFact]
    public void Defaults_are_expected()
    {
        MessageInput input = new MessageInput();

        Assert.Null(input.Text);
        Assert.Equal("Message", input.Watermark);
        Assert.Equal(5, input.MaxLines);
        Assert.Null(input.SendCommand);
    }

    [AvaloniaFact]
    public void Template_parts_are_bound()
    {
        MessageInput input = new MessageInput { Text = "Hello", Watermark = "Type here" };
        Show(input);

        TextBox textBox = Part<TextBox>(input, "PART_TextBox");
        Assert.Equal("Hello", textBox.Text);
        Assert.Equal("Type here", textBox.PlaceholderText);
        Assert.Equal(5, textBox.MaxLines);
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
        ((global::Avalonia.Controls.IPseudoClasses)textBox.Classes).Set(":pointerover", true);
        Dispatcher.UIThread.RunJobs();

        Border frame = input.GetVisualDescendants().OfType<Border>().First(x => x.Name == "PART_BorderElement");
        Assert.Equal(new Thickness(0), frame.BorderThickness);
    }

    [AvaloniaFact]
    public void Send_icon_is_themed_and_replaceable()
    {
        MessageInput input = new MessageInput();
        Show(input);

        ShapePath icon = input.GetVisualDescendants().OfType<ShapePath>().First();
        Assert.NotNull(input.SendIcon);
        Assert.Same(input.SendIcon, icon.Data);

        Geometry custom = Geometry.Parse("M0 0 L1 1");
        input.SendIcon = custom;
        Dispatcher.UIThread.RunJobs();

        Assert.Same(custom, input.GetVisualDescendants().OfType<ShapePath>().First().Data);
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