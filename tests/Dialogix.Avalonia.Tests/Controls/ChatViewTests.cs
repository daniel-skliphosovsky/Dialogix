using System.Collections.ObjectModel;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Dialogix.Avalonia.Controls;

namespace Dialogix.Avalonia.Tests.Controls;

public class ChatViewTests
{
    [AvaloniaFact]
    public void Defaults_are_expected()
    {
        ChatView chat = new ChatView();

        Assert.Null(chat.Messages);
        Assert.Null(chat.MessageTemplate);
        Assert.False(chat.HasMore);
        Assert.False(chat.IsLoadingMore);
        Assert.Null(chat.LoadMoreCommand);
        Assert.Equal(48d, chat.LoadMoreThreshold);
    }

    [AvaloniaFact]
    public void Messages_are_bound_to_items_control()
    {
        ObservableCollection<MessageBubble> messages = [new MessageBubble { Text = "Hello" }];
        ChatView chat = new ChatView { Messages = messages };
        Show(chat);

        ItemsControl items = Part<ItemsControl>(chat, "PART_Items");
        Assert.Same(messages, items.ItemsSource);
    }

    [AvaloniaFact]
    public void Message_template_renders_data_items_as_bubbles()
    {
        ObservableCollection<ChatMessage> messages = [new ChatMessage("Hello", "Guest")];
        ChatView chat = new ChatView
        {
            Messages = messages,
            MessageTemplate = new FuncDataTemplate<ChatMessage>((message, _) => new MessageBubble
            {
                Text = message.Text,
                Author = message.Author
            })
        };
        Show(chat);

        MessageBubble bubble = chat.GetVisualDescendants().OfType<MessageBubble>().Single();
        Assert.Equal("Hello", bubble.Text);
        Assert.Equal("Guest", bubble.Author);
    }

    [AvaloniaFact]
    public void Message_template_can_render_any_control()
    {
        ChatView chat = new ChatView
        {
            Messages = new[] { new ChatMessage("Hello", "Guest") },
            MessageTemplate = new FuncDataTemplate<ChatMessage>((message, _) => new TextBlock { Text = message.Text })
        };
        Show(chat);

        TextBlock rendered = chat.GetVisualDescendants().OfType<TextBlock>().Single(x => x.Text == "Hello");
        Assert.Equal("Hello", rendered.Text);
    }

    [AvaloniaFact]
    public void Message_template_keeps_up_with_the_collection()
    {
        ObservableCollection<ChatMessage> messages = [new ChatMessage("First", "Guest")];
        ChatView chat = new ChatView
        {
            Messages = messages,
            MessageTemplate = new FuncDataTemplate<ChatMessage>((message, _) => new MessageBubble { Text = message.Text })
        };
        Show(chat);

        messages.Add(new ChatMessage("Second", "You"));
        Dispatcher.UIThread.RunJobs();

        string[] texts = chat.GetVisualDescendants().OfType<MessageBubble>().Select(x => x.Text ?? string.Empty).ToArray();
        Assert.Equal(["First", "Second"], texts);
    }

    [AvaloniaFact]
    public void Chat_view_leaves_the_composer_to_the_app()
    {
        ChatView chat = new ChatView();
        Show(chat);

        Assert.Empty(chat.GetVisualDescendants().OfType<MessageInput>());
    }

    [AvaloniaFact]
    public void Message_input_can_live_next_to_the_chat_view()
    {
        ObservableCollection<MessageBubble> messages = ManyMessages(3);
        ChatView chat = new ChatView { Messages = messages };
        MessageInput input = new MessageInput();

        input.SendCommand = new ActionCommand(parameter =>
        {
            messages.Add(new MessageBubble { Text = parameter as string ?? string.Empty });
            chat.ScrollToEnd();
        });

        StackPanel shell = new StackPanel { Children = { chat, input } };
        Window window = new Window { Content = shell, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        TextBox textBox = input.GetVisualDescendants().OfType<TextBox>().First(x => x.Name == "PART_TextBox");
        textBox.Text = "Hi";
        Dispatcher.UIThread.RunJobs();

        textBox.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Enter,
            KeyModifiers = KeyModifiers.None,
            Source = textBox
        });

        Assert.Equal(4, messages.Count);
        Assert.Equal(string.Empty, input.Text);
    }

    [AvaloniaFact]
    public void Scroll_to_end_returns_to_the_latest_message()
    {
        ChatView chat = new ChatView { Messages = ManyMessages(40) };
        Show(chat, 400, 200);

        ScrollViewer scroll = Part<ScrollViewer>(chat, "PART_Scroll");
        scroll.Offset = new Vector(0, 0);
        Dispatcher.UIThread.RunJobs();

        chat.ScrollToEnd();
        Dispatcher.UIThread.RunJobs();

        double max = scroll.Extent.Height - scroll.Viewport.Height;
        Assert.Equal(max, scroll.Offset.Y, 1d);
    }

    [AvaloniaFact]
    public void Opens_scrolled_to_the_latest_message()
    {
        ChatView chat = new ChatView { Messages = ManyMessages(40) };
        Show(chat, 400, 200);

        ScrollViewer scroll = Part<ScrollViewer>(chat, "PART_Scroll");
        double max = scroll.Extent.Height - scroll.Viewport.Height;

        Assert.True(max > 0);
        Assert.Equal(max, scroll.Offset.Y, 1d);
    }

    [AvaloniaFact]
    public void New_message_scrolls_to_the_bottom_when_already_there()
    {
        ObservableCollection<MessageBubble> messages = ManyMessages(40);
        ChatView chat = new ChatView { Messages = messages };
        Show(chat, 400, 200);

        ScrollViewer scroll = Part<ScrollViewer>(chat, "PART_Scroll");
        messages.Add(new MessageBubble { Text = "New message" });
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        double max = scroll.Extent.Height - scroll.Viewport.Height;
        Assert.Equal(max, scroll.Offset.Y, 1d);
    }

    [AvaloniaFact]
    public void New_message_does_not_scroll_when_the_user_reads_history()
    {
        ObservableCollection<MessageBubble> messages = ManyMessages(40);
        ChatView chat = new ChatView { Messages = messages, SmoothScrolling = false };
        Show(chat, 400, 200);

        ScrollViewer scroll = Part<ScrollViewer>(chat, "PART_Scroll");
        scroll.Offset = new Vector(0, 0);
        Dispatcher.UIThread.RunJobs();

        messages.Add(new MessageBubble { Text = "New message" });
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0d, scroll.Offset.Y, 1d);
    }

    [AvaloniaFact]
    public void Load_more_runs_at_the_top_when_more_messages_exist()
    {
        Recorder recorder = new Recorder();
        ChatView chat = new ChatView
        {
            Messages = ManyMessages(40),
            HasMore = true,
            LoadMoreCommand = recorder
        };

        Show(chat, 400, 200);

        ScrollViewer scroll = Part<ScrollViewer>(chat, "PART_Scroll");
        scroll.Offset = new Vector(0, 0);
        Dispatcher.UIThread.RunJobs();

        Assert.Single(recorder.Executed);
    }

    [AvaloniaFact]
    public void Load_more_runs_only_once_until_the_user_scrolls_away()
    {
        Recorder recorder = new Recorder();
        ChatView chat = new ChatView
        {
            Messages = ManyMessages(40),
            HasMore = true,
            LoadMoreCommand = recorder
        };

        Show(chat, 400, 200);

        ScrollViewer scroll = Part<ScrollViewer>(chat, "PART_Scroll");
        scroll.Offset = new Vector(0, 0);
        Dispatcher.UIThread.RunJobs();
        scroll.Offset = new Vector(0, 1);
        Dispatcher.UIThread.RunJobs();

        Assert.Single(recorder.Executed);
    }

    [AvaloniaFact]
    public void Load_more_does_not_run_when_no_more_messages_exist()
    {
        Recorder recorder = new Recorder();
        ChatView chat = new ChatView
        {
            Messages = ManyMessages(40),
            HasMore = false,
            LoadMoreCommand = recorder
        };

        Show(chat, 400, 200);

        ScrollViewer scroll = Part<ScrollViewer>(chat, "PART_Scroll");
        scroll.Offset = new Vector(0, 0);
        Dispatcher.UIThread.RunJobs();

        Assert.Empty(recorder.Executed);
    }

    [AvaloniaFact]
    public void Load_more_fills_a_viewport_that_does_not_overflow()
    {
        Recorder recorder = new Recorder();
        ChatView chat = new ChatView
        {
            Messages = ManyMessages(2),
            HasMore = true,
            LoadMoreCommand = recorder
        };

        Show(chat, 400, 300);
        Dispatcher.UIThread.RunJobs();

        Assert.Single(recorder.Executed);
    }

    [AvaloniaFact]
    public void Load_more_can_run_again_after_messages_are_replaced()
    {
        Recorder recorder = new Recorder();
        ChatView chat = new ChatView
        {
            Messages = ManyMessages(40),
            HasMore = true,
            LoadMoreCommand = recorder
        };

        Show(chat, 400, 200);

        ScrollViewer scroll = Part<ScrollViewer>(chat, "PART_Scroll");
        scroll.Offset = new Vector(0, 0);
        Dispatcher.UIThread.RunJobs();
        Assert.Single(recorder.Executed);

        chat.Messages = ManyMessages(2);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(2, recorder.Executed.Count);
    }

    [AvaloniaFact]
    public void Load_more_runs_when_older_messages_become_available_at_the_top()
    {
        Recorder recorder = new Recorder();
        ChatView chat = new ChatView
        {
            Messages = ManyMessages(40),
            HasMore = false,
            LoadMoreCommand = recorder
        };

        Show(chat, 400, 200);

        ScrollViewer scroll = Part<ScrollViewer>(chat, "PART_Scroll");
        scroll.Offset = new Vector(0, 0);
        Dispatcher.UIThread.RunJobs();
        Assert.Empty(recorder.Executed);

        chat.HasMore = true;
        Dispatcher.UIThread.RunJobs();

        Assert.Single(recorder.Executed);
    }

    [AvaloniaFact]
    public void Prepended_messages_keep_the_scroll_anchor()
    {
        ObservableCollection<MessageBubble> messages = ManyMessages(40);
        ChatView chat = new ChatView { Messages = messages, HasMore = true, LoadMoreCommand = new ActionCommand(() => messages.Insert(0, new MessageBubble { Text = "Older" })) };
        Show(chat, 400, 200);

        ScrollViewer scroll = Part<ScrollViewer>(chat, "PART_Scroll");
        double extent = scroll.Extent.Height;

        scroll.Offset = new Vector(0, 0);
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        double added = scroll.Extent.Height - extent;
        Assert.True(added > 0);
        Assert.Equal(added, scroll.Offset.Y, 1d);
    }

    [AvaloniaFact]
    public void Scroll_to_end_icon_is_themed()
    {
        ChatView chat = new ChatView();
        Show(chat);

        Assert.NotNull(chat.ScrollToEndIcon);
    }

    [AvaloniaFact]
    public void Scroll_to_end_button_is_hidden_at_the_bottom()
    {
        ChatView chat = new ChatView { Messages = ManyMessages(40) };
        Show(chat, 400, 200);

        Assert.False(Part<Button>(chat, "PART_ScrollToEnd").IsVisible);
    }

    [AvaloniaFact]
    public void Scroll_to_end_button_returns_to_the_latest_message()
    {
        ChatView chat = new ChatView { Messages = ManyMessages(40), SmoothScrolling = false };
        Show(chat, 400, 200);

        ScrollViewer scroll = Part<ScrollViewer>(chat, "PART_Scroll");
        Button button = Part<Button>(chat, "PART_ScrollToEnd");

        scroll.Offset = new Vector(0, 0);
        Dispatcher.UIThread.RunJobs();
        Assert.True(button.IsVisible);

        double max = scroll.Extent.Height - scroll.Viewport.Height;
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(max, scroll.Offset.Y, 1d);
        Assert.False(button.IsVisible);
    }

    private sealed record ChatMessage(string Text, string Author);

    private static ObservableCollection<MessageBubble> ManyMessages(int count)
    {
        ObservableCollection<MessageBubble> messages = [];

        for (int i = 0; i < count; i++)
            messages.Add(new MessageBubble { Text = $"Message {i}" });

        return messages;
    }

    private static void Show(Control control, double width = 400, double height = 300)
    {
        Window window = new Window { Content = control, Width = width, Height = height };
        window.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static T Part<T>(Control control, string name) where T : Control =>
        control.GetVisualDescendants().OfType<T>().First(x => x.Name == name);

    private sealed class ActionCommand : ICommand
    {
        private readonly Action<object?> _action;

        public ActionCommand(Action action) => _action = _ => action();

        public ActionCommand(Action<object?> action) => _action = action;

        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _action(parameter);
    }

    private sealed class Recorder : ICommand
    {
        public List<object?> Executed { get; } = [];

        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => Executed.Add(parameter);
    }
}