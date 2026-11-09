using System.Collections.ObjectModel;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Dialogix.Avalonia.Controls;

namespace Dialogix.Avalonia.Tests.Controls.Chat;

public class MessengerViewTests
{
    [AvaloniaFact]
    public void Defaults_are_expected()
    {
        MessengerView view = new MessengerView();

        Assert.Null(view.Conversations);
        Assert.Null(view.ConversationTemplate);
        Assert.Null(view.SelectedConversation);
        Assert.Null(view.ConversationsEmptyContent);
        Assert.Null(view.ConversationsEmptyContentTemplate);
        Assert.Null(view.Messages);
        Assert.Null(view.MessageTemplate);
        Assert.Null(view.Title);
        Assert.Null(view.Status);
        Assert.Equal("Select a conversation", view.PlaceholderText);
        Assert.Equal("Message", view.Watermark);
        Assert.Null(view.SendCommand);
        Assert.False(view.HasMore);
        Assert.False(view.IsLoadingMore);
        Assert.Null(view.LoadMoreCommand);
        Assert.True(view.IsListVisible);
        Assert.Equal(280d, view.ListWidth);
        Assert.Null(view.HeaderLeadingContent);
        Assert.Null(view.HeaderLeadingContentTemplate);
        Assert.Null(view.HeaderActionContent);
        Assert.Null(view.HeaderActionContentTemplate);
        Assert.Null(view.ComposerLeadingContent);
        Assert.Null(view.ComposerLeadingContentTemplate);
        Assert.Null(view.ComposerActionContent);
        Assert.Null(view.ComposerActionContentTemplate);
        Assert.False(view.HasConversation);
    }

    [AvaloniaFact]
    public void Child_controls_receive_the_properties()
    {
        ObservableCollection<string> conversations = ["Nina", "Alex"];
        ObservableCollection<string> messages = ["Hello", "Hi"];
        IDataTemplate conversationTemplate = new FuncDataTemplate<object>((item, _) => new TextBlock { Text = item?.ToString() });
        IDataTemplate messageTemplate = new FuncDataTemplate<object>((item, _) => new TextBlock { Text = item?.ToString() });
        Recorder send = new Recorder();
        Recorder loadMore = new Recorder();
        TextBlock leading = new() { Text = "Attach" };
        TextBlock action = new() { Text = "Emoji" };
        Button headerAction = new() { Content = "More" };

        MessengerView view = new MessengerView
        {
            Conversations = conversations,
            ConversationTemplate = conversationTemplate,
            SelectedConversation = "Nina",
            Messages = messages,
            MessageTemplate = messageTemplate,
            Title = "Nina",
            Status = "Online",
            Watermark = "Write",
            SendCommand = send,
            HasMore = true,
            IsLoadingMore = true,
            LoadMoreCommand = loadMore,
            HeaderActionContent = headerAction,
            ComposerLeadingContent = leading,
            ComposerActionContent = action
        };
        Show(view);

        ConversationListView list = Part<ConversationListView>(view, "PART_List");
        ChatView chat = Part<ChatView>(view, "PART_Chat");
        ChatHeader header = Part<ChatHeader>(view, "PART_Header");
        MessageInput input = Part<MessageInput>(view, "PART_Input");

        Assert.Same(conversations, list.Items);
        Assert.Same(conversationTemplate, list.ItemTemplate);
        Assert.Equal("Nina", list.SelectedConversation);

        Assert.Same(messages, chat.Messages);
        Assert.Same(messageTemplate, chat.MessageTemplate);
        Assert.True(chat.HasMore);
        Assert.True(chat.IsLoadingMore);
        Assert.Same(loadMore, chat.LoadMoreCommand);

        Assert.Equal("Nina", header.Title);
        Assert.Equal("Online", header.Status);
        Assert.Same(headerAction, header.ActionContent);

        Assert.Equal("Write", input.Watermark);
        Assert.Same(send, input.SendCommand);
        Assert.Same(leading, input.LeadingContent);
        Assert.Same(action, input.ActionContent);
    }

    [AvaloniaFact]
    public void Selection_flows_both_ways_and_toggles_the_placeholder()
    {
        MessengerView view = new MessengerView
        {
            Conversations = new[] { "Nina", "Alex" },
            ConversationTemplate = new FuncDataTemplate<object>((item, _) => new TextBlock { Text = item?.ToString() })
        };
        Show(view);

        Grid conversation = Part<Grid>(view, "PART_Conversation");
        TextBlock placeholder = Part<TextBlock>(view, "PART_Placeholder");
        ConversationListView list = Part<ConversationListView>(view, "PART_List");

        Assert.False(view.HasConversation);
        Assert.False(conversation.IsVisible);
        Assert.True(placeholder.IsVisible);

        list.SelectedConversation = "Nina";
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Nina", view.SelectedConversation);
        Assert.True(view.HasConversation);
        Assert.True(conversation.IsVisible);
        Assert.False(placeholder.IsVisible);

        view.SelectedConversation = null;
        Dispatcher.UIThread.RunJobs();

        Assert.False(view.HasConversation);
        Assert.False(conversation.IsVisible);
        Assert.True(placeholder.IsVisible);
    }

    [AvaloniaFact]
    public void List_can_be_resized_and_hidden()
    {
        MessengerView view = new MessengerView { ListWidth = 320 };
        Show(view);

        Border host = Part<Border>(view, "PART_ListHost");
        Assert.Equal(320d, host.Bounds.Width);

        view.IsListVisible = false;
        Dispatcher.UIThread.RunJobs();

        Assert.False(host.IsVisible);
    }

    [AvaloniaFact]
    public void Empty_state_is_forwarded_to_the_list()
    {
        TextBlock empty = new() { Text = "No conversations" };
        IDataTemplate template = new FuncDataTemplate<object>((_, _) => new TextBlock());
        MessengerView view = new MessengerView
        {
            ConversationsEmptyContent = empty,
            ConversationsEmptyContentTemplate = template
        };
        Show(view);

        ConversationListView list = Part<ConversationListView>(view, "PART_List");
        Assert.Same(empty, list.EmptyContent);
        Assert.Same(template, list.EmptyContentTemplate);
    }

    private static void Show(Control control)
    {
        Window window = new Window { Content = control, Width = 800, Height = 600 };
        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static T Part<T>(Control control, string name) where T : Control =>
        control.GetVisualDescendants().OfType<T>().First(x => x.Name == name);

    private sealed class Recorder : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) { }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}