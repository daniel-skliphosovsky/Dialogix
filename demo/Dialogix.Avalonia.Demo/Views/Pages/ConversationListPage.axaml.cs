using Avalonia.Controls;

using Dialogix.Avalonia.Controls;

namespace Dialogix.Avalonia.Demo.Views.Pages;

public sealed record ConversationItem(
    string Name,
    string Initials,
    string Preview,
    string Time,
    bool IsUnread,
    string Unread);

public partial class ConversationListPage : UserControl
{
    public ConversationListPage()
    {
        InitializeComponent();

        ConversationItem[] items =
        [
            new("Nina", "N", "Sent the updated mockups.", "12:41", true, "2"),
            new("Alex", "A", "See you at the standup.", "12:20", false, ""),
            new("Maya", "M", "The build is green again.", "11:58", false, ""),
            new("Project room", "PR", "Denis: pushed the fix.", "11:32", true, "5"),
            new("Support", "S", "Ticket #1842 was resolved.", "10:47", false, ""),
            new("Leo", "L", "Thanks for the review!", "09:15", false, ""),
            new("Design", "D", "New palette is in the repo.", "Yesterday", false, ""),
            new("Ira", "I", "Can you send the link?", "Yesterday", false, ""),
            new("Max", "M", "Sounds good.", "Mon", false, ""),
            new("Release", "R", "v0.1.0-alpha is out.", "Mon", false, "")
        ];

        Conversations.Items = items;
        Conversations.SelectedConversation = items[0];
        Conversations.PropertyChanged += (_, e) =>
        {
            if (e.Property == ConversationListView.SelectedConversationProperty)
                UpdateStatus();
        };

        UpdateStatus();

        EmptyConversations.Items = Array.Empty<ConversationItem>();
    }

    private void UpdateStatus() =>
        SelectedStatus.Text = Conversations.SelectedConversation is ConversationItem item
            ? $"Selected: {item.Name}"
            : "Nothing selected.";
}