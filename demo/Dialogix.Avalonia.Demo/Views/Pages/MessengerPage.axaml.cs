using System.Collections.ObjectModel;

using Avalonia.Controls;

using Dialogix.Avalonia.Controls;
using Dialogix.Avalonia.Demo.Infrastructure;

namespace Dialogix.Avalonia.Demo.Views.Pages;

public sealed record MessengerMessage(string Author, string Text, string Time, bool IsOutgoing);

public sealed class MessengerConversation(
    string name,
    string initials,
    string preview,
    string time,
    bool isUnread,
    string unread)
{
    public string Name { get; } = name;
    public string Initials { get; } = initials;
    public string Preview { get; } = preview;
    public string Time { get; } = time;
    public bool IsUnread { get; } = isUnread;
    public string Unread { get; } = unread;
    public ObservableCollection<MessengerMessage> Messages { get; } = [];
}

public partial class MessengerPage : UserControl
{
    public MessengerPage()
    {
        InitializeComponent();

        MessengerConversation[] conversations =
        [
            Conversation("Nina", "N", "Sent the updated mockups.", "12:41", true, "2",
                ("Nina", "Hey! The new chat list looks great.", "12:38", false),
                ("You", "Thanks. It is virtualized now.", "12:39", true),
                ("Nina", "Sent the updated mockups.", "12:41", false)),
            Conversation("Alex", "A", "See you at the standup.", "12:20", false, "",
                ("Alex", "See you at the standup.", "12:20", false),
                ("You", "On my way.", "12:21", true)),
            Conversation("Maya", "M", "The build is green again.", "11:58", false, "",
                ("Maya", "The build is green again.", "11:58", false)),
            Conversation("Project room", "PR", "Denis: pushed the fix.", "11:32", true, "5",
                ("Denis", "Pushed the fix.", "11:32", false)),
            Conversation("Support", "S", "Ticket #1842 was resolved.", "10:47", false, "",
                ("Support", "Ticket #1842 was resolved.", "10:47", false))
        ];

        Messenger.Conversations = conversations;
        Messenger.SendCommand = new RelayCommand(parameter =>
        {
            if (parameter is not string text
                || string.IsNullOrWhiteSpace(text)
                || Messenger.SelectedConversation is not MessengerConversation conversation)
            {
                return;
            }

            conversation.Messages.Add(new MessengerMessage("You", text, "Now", true));
        });

        Messenger.PropertyChanged += (_, e) =>
        {
            if (e.Property == MessengerView.SelectedConversationProperty)
                ShowConversation();
        };

        Messenger.SelectedConversation = conversations[0];
        ShowConversation();
    }

    private void ShowConversation()
    {
        if (Messenger.SelectedConversation is not MessengerConversation conversation)
        {
            Messenger.Messages = null;
            return;
        }

        Messenger.Title = conversation.Name;
        Messenger.Status = conversation.IsUnread ? "New messages" : "Online";
        Messenger.Messages = conversation.Messages;
    }

    private static MessengerConversation Conversation(
        string name,
        string initials,
        string preview,
        string time,
        bool isUnread,
        string unread,
        params (string Author, string Text, string Time, bool IsOutgoing)[] messages)
    {
        MessengerConversation conversation = new(name, initials, preview, time, isUnread, unread);

        foreach ((string author, string text, string messageTime, bool outgoing) in messages)
            conversation.Messages.Add(new MessengerMessage(author, text, messageTime, outgoing));

        return conversation;
    }
}