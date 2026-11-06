using System.Collections.ObjectModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

using Dialogix.Avalonia.Controls;

namespace Dialogix.Avalonia.Demo.Views.Controls;

public sealed record DemoMessage(string Author, string Text, string Time, bool IsOutgoing);

public partial class ChatViewView : UserControl
{
    private static readonly (string Author, string Text)[] DefaultSeed =
    [
        ("Guest", "Hey! Have you seen these chat controls?"),
        ("You", "Just added Dialogix to the project."),
        ("Guest", "So you no longer rebuild bubbles in every screen?"),
        ("You", "Right. One control, and the theme handles the rest."),
        ("Guest", "Does it do pagination?"),
        ("You", "Scroll up — older messages load in."),
        ("Guest", "Nice. What about the input?"),
        ("You", "Enter sends, Shift+Enter adds a line."),
        ("Guest", "And the outgoing bubbles?"),
        ("You", "Same control, just IsOutgoing."),
        ("Guest", "Clean."),
        ("You", "Try sending something.")
    ];

    private static readonly (string Author, string Text)[] VioletSeed =
    [
        ("Guest", "Dark background, violet accent."),
        ("You", "Every brush is a resource."),
        ("Guest", "Even the send button?"),
        ("You", "It picks up the scoped primary color."),
        ("Guest", "Send something and scroll up.")
    ];

    private static readonly (string Author, string Text)[] MonoSeed =
    [
        ("Guest", "Monospace, square corners."),
        ("You", "Restyled with plain selectors."),
        ("Guest", "No custom control needed."),
        ("You", "And the scroll-down button still works.")
    ];

    private static readonly (string Author, string Text)[] TemplateSeed =
    [
        ("Guest", "This card keeps plain view models."),
        ("You", "The template turns them into bubbles."),
        ("Guest", "No control has to know about the data."),
        ("You", "Try sending something.")
    ];

    public ChatViewView()
    {
        InitializeComponent();

        Setup(ChatDesktop, InputDesktop, DefaultSeed, 3);
        Setup(ChatDefault, InputDefault, DefaultSeed, 3);
        Setup(ChatViolet, InputViolet, VioletSeed, 2);
        Setup(ChatMono, InputMono, MonoSeed, 2);
        SetupTemplate(ChatTemplate, InputTemplate, TemplateSeed, 2);

        SizeChanged += OnSizeChanged;

    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e) => ApplyWidth(e.NewSize.Width);

    private void ApplyWidth(double width)
    {
        if (PageContent is null)
            return;

        bool narrow = width < 1000;
        double margin = narrow ? 16 : 24;
        double available = Math.Max(240, width - (margin * 2) - 1);

        PageContent.Margin = new Thickness(margin);
        PageTitle.FontSize = narrow ? 22 : 26;
        Cards.ItemWidth = narrow ? available : 326;

        foreach (Border card in Cards.Children.OfType<Border>())
            card.Margin = narrow ? new Thickness(0, 0, 0, 12) : new Thickness(0, 0, 10, 10);
    }

    private static void Setup(ChatView chat, MessageInput input, (string Author, string Text)[] seed, int pages)
    {
        ObservableCollection<MessageBubble> messages = [];
        int pagesLeft = pages;
        int index = 0;

        foreach ((string author, string text) in seed)
            messages.Add(CreateBubble(author, text, index++));

        chat.Messages = messages;
        chat.HasMore = pages > 0;

        chat.LoadMoreCommand = new RelayCommand(_ =>
        {
            if (pagesLeft <= 0)
            {
                chat.HasMore = false;
                return;
            }

            pagesLeft--;
            chat.HasMore = pagesLeft > 0;
            chat.IsLoadingMore = true;

            DispatcherTimer timer = new(TimeSpan.FromMilliseconds(600), DispatcherPriority.Normal, Dispatcher.UIThread);
            timer.Tick += (_, _) =>
            {
                timer.Stop();

                for (int i = 0; i < 4; i++)
                    messages.Insert(0, CreateBubble("Guest", $"Older message #{i + 1}", 0));

                chat.IsLoadingMore = false;
            };
            timer.Start();
        });

        input.SendCommand = new RelayCommand(parameter =>
        {
            if (parameter is not string text || string.IsNullOrWhiteSpace(text))
                return;

            messages.Add(CreateBubble("You", text, messages.Count));
            chat.ScrollToEnd();
        });
    }

    private static void SetupTemplate(ChatView chat, MessageInput input, (string Author, string Text)[] seed, int pages)
    {
        ObservableCollection<DemoMessage> messages = [];
        int pagesLeft = pages;
        int index = 0;

        foreach ((string author, string text) in seed)
            messages.Add(CreateMessage(author, text, index++));

        chat.Messages = messages;
        chat.HasMore = pages > 0;

        chat.LoadMoreCommand = new RelayCommand(_ =>
        {
            if (pagesLeft <= 0)
            {
                chat.HasMore = false;
                return;
            }

            pagesLeft--;
            chat.HasMore = pagesLeft > 0;

            for (int i = 0; i < 4; i++)
                messages.Insert(0, CreateMessage("Guest", $"Older message #{i + 1}", 0));
        });

        input.SendCommand = new RelayCommand(parameter =>
        {
            if (parameter is not string text || string.IsNullOrWhiteSpace(text))
                return;

            messages.Add(CreateMessage("You", text, messages.Count));
            chat.ScrollToEnd();
        });
    }

    private static DemoMessage CreateMessage(string author, string text, int index) =>
        new(author, text, $"12:{index:D2}", author == "You");

    private static MessageBubble CreateBubble(string author, string text, int index) => new()
    {
        Author = author,
        Text = text,
        Timestamp = $"12:{index:D2}",
        IsOutgoing = author == "You"
    };
}