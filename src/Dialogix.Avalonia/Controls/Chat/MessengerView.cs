using System.Collections;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace Dialogix.Avalonia.Controls;

/// <summary>
/// A ready messenger layout: a conversation list, the active chat and a composer.
/// </summary>
public class MessengerView : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> ConversationsProperty =
        AvaloniaProperty.Register<MessengerView, IEnumerable?>(nameof(Conversations));

    public static readonly StyledProperty<IDataTemplate?> ConversationTemplateProperty =
        AvaloniaProperty.Register<MessengerView, IDataTemplate?>(nameof(ConversationTemplate));

    public static readonly StyledProperty<object?> SelectedConversationProperty =
        AvaloniaProperty.Register<MessengerView, object?>(
            nameof(SelectedConversation),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object?> ConversationsEmptyContentProperty =
        AvaloniaProperty.Register<MessengerView, object?>(nameof(ConversationsEmptyContent));

    public static readonly StyledProperty<IDataTemplate?> ConversationsEmptyContentTemplateProperty =
        AvaloniaProperty.Register<MessengerView, IDataTemplate?>(nameof(ConversationsEmptyContentTemplate));

    public static readonly StyledProperty<IEnumerable?> MessagesProperty =
        AvaloniaProperty.Register<MessengerView, IEnumerable?>(nameof(Messages));

    public static readonly StyledProperty<IDataTemplate?> MessageTemplateProperty =
        AvaloniaProperty.Register<MessengerView, IDataTemplate?>(nameof(MessageTemplate));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<MessengerView, string?>(nameof(Title));

    public static readonly StyledProperty<string?> StatusProperty =
        AvaloniaProperty.Register<MessengerView, string?>(nameof(Status));

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<MessengerView, string?>(nameof(PlaceholderText), "Select a conversation");

    public static readonly StyledProperty<string?> WatermarkProperty =
        AvaloniaProperty.Register<MessengerView, string?>(nameof(Watermark), "Message");

    public static readonly StyledProperty<ICommand?> SendCommandProperty =
        AvaloniaProperty.Register<MessengerView, ICommand?>(nameof(SendCommand));

    public static readonly StyledProperty<bool> HasMoreProperty =
        AvaloniaProperty.Register<MessengerView, bool>(nameof(HasMore));

    public static readonly StyledProperty<bool> IsLoadingMoreProperty =
        AvaloniaProperty.Register<MessengerView, bool>(nameof(IsLoadingMore));

    public static readonly StyledProperty<ICommand?> LoadMoreCommandProperty =
        AvaloniaProperty.Register<MessengerView, ICommand?>(nameof(LoadMoreCommand));

    public static readonly StyledProperty<bool> IsListVisibleProperty =
        AvaloniaProperty.Register<MessengerView, bool>(nameof(IsListVisible), true);

    public static readonly StyledProperty<double> ListWidthProperty =
        AvaloniaProperty.Register<MessengerView, double>(nameof(ListWidth), 280d);

    public static readonly StyledProperty<object?> HeaderLeadingContentProperty =
        AvaloniaProperty.Register<MessengerView, object?>(nameof(HeaderLeadingContent));

    public static readonly StyledProperty<IDataTemplate?> HeaderLeadingContentTemplateProperty =
        AvaloniaProperty.Register<MessengerView, IDataTemplate?>(nameof(HeaderLeadingContentTemplate));

    public static readonly StyledProperty<object?> HeaderActionContentProperty =
        AvaloniaProperty.Register<MessengerView, object?>(nameof(HeaderActionContent));

    public static readonly StyledProperty<IDataTemplate?> HeaderActionContentTemplateProperty =
        AvaloniaProperty.Register<MessengerView, IDataTemplate?>(nameof(HeaderActionContentTemplate));

    public static readonly StyledProperty<object?> ComposerLeadingContentProperty =
        AvaloniaProperty.Register<MessengerView, object?>(nameof(ComposerLeadingContent));

    public static readonly StyledProperty<IDataTemplate?> ComposerLeadingContentTemplateProperty =
        AvaloniaProperty.Register<MessengerView, IDataTemplate?>(nameof(ComposerLeadingContentTemplate));

    public static readonly StyledProperty<object?> ComposerActionContentProperty =
        AvaloniaProperty.Register<MessengerView, object?>(nameof(ComposerActionContent));

    public static readonly StyledProperty<IDataTemplate?> ComposerActionContentTemplateProperty =
        AvaloniaProperty.Register<MessengerView, IDataTemplate?>(nameof(ComposerActionContentTemplate));

    public static readonly DirectProperty<MessengerView, bool> HasConversationProperty =
        AvaloniaProperty.RegisterDirect<MessengerView, bool>(
            nameof(HasConversation),
            view => view.HasConversation);

    private bool _hasConversation;

    public IEnumerable? Conversations { get => GetValue(ConversationsProperty); set => SetValue(ConversationsProperty, value); }
    public IDataTemplate? ConversationTemplate { get => GetValue(ConversationTemplateProperty); set => SetValue(ConversationTemplateProperty, value); }
    public object? SelectedConversation { get => GetValue(SelectedConversationProperty); set => SetValue(SelectedConversationProperty, value); }
    public object? ConversationsEmptyContent { get => GetValue(ConversationsEmptyContentProperty); set => SetValue(ConversationsEmptyContentProperty, value); }
    public IDataTemplate? ConversationsEmptyContentTemplate { get => GetValue(ConversationsEmptyContentTemplateProperty); set => SetValue(ConversationsEmptyContentTemplateProperty, value); }
    public IEnumerable? Messages { get => GetValue(MessagesProperty); set => SetValue(MessagesProperty, value); }
    public IDataTemplate? MessageTemplate { get => GetValue(MessageTemplateProperty); set => SetValue(MessageTemplateProperty, value); }
    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string? Status { get => GetValue(StatusProperty); set => SetValue(StatusProperty, value); }
    public string? PlaceholderText { get => GetValue(PlaceholderTextProperty); set => SetValue(PlaceholderTextProperty, value); }
    public string? Watermark { get => GetValue(WatermarkProperty); set => SetValue(WatermarkProperty, value); }
    public ICommand? SendCommand { get => GetValue(SendCommandProperty); set => SetValue(SendCommandProperty, value); }
    public bool HasMore { get => GetValue(HasMoreProperty); set => SetValue(HasMoreProperty, value); }
    public bool IsLoadingMore { get => GetValue(IsLoadingMoreProperty); set => SetValue(IsLoadingMoreProperty, value); }
    public ICommand? LoadMoreCommand { get => GetValue(LoadMoreCommandProperty); set => SetValue(LoadMoreCommandProperty, value); }
    public bool IsListVisible { get => GetValue(IsListVisibleProperty); set => SetValue(IsListVisibleProperty, value); }
    public double ListWidth { get => GetValue(ListWidthProperty); set => SetValue(ListWidthProperty, value); }
    public object? HeaderLeadingContent { get => GetValue(HeaderLeadingContentProperty); set => SetValue(HeaderLeadingContentProperty, value); }
    public IDataTemplate? HeaderLeadingContentTemplate { get => GetValue(HeaderLeadingContentTemplateProperty); set => SetValue(HeaderLeadingContentTemplateProperty, value); }
    public object? HeaderActionContent { get => GetValue(HeaderActionContentProperty); set => SetValue(HeaderActionContentProperty, value); }
    public IDataTemplate? HeaderActionContentTemplate { get => GetValue(HeaderActionContentTemplateProperty); set => SetValue(HeaderActionContentTemplateProperty, value); }
    public object? ComposerLeadingContent { get => GetValue(ComposerLeadingContentProperty); set => SetValue(ComposerLeadingContentProperty, value); }
    public IDataTemplate? ComposerLeadingContentTemplate { get => GetValue(ComposerLeadingContentTemplateProperty); set => SetValue(ComposerLeadingContentTemplateProperty, value); }
    public object? ComposerActionContent { get => GetValue(ComposerActionContentProperty); set => SetValue(ComposerActionContentProperty, value); }
    public IDataTemplate? ComposerActionContentTemplate { get => GetValue(ComposerActionContentTemplateProperty); set => SetValue(ComposerActionContentTemplateProperty, value); }
    public bool HasConversation { get => _hasConversation; private set => SetAndRaise(HasConversationProperty, ref _hasConversation, value); }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedConversationProperty)
            HasConversation = change.GetNewValue<object?>() is not null;
    }
}