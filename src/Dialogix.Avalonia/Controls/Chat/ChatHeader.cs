using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace Dialogix.Avalonia.Controls;

public class ChatHeader : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ChatHeader, string?>(nameof(Title));

    public static readonly StyledProperty<string?> StatusProperty =
        AvaloniaProperty.Register<ChatHeader, string?>(nameof(Status));

    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        AvaloniaProperty.Register<ChatHeader, BoxShadows>(nameof(BoxShadow));

    public static readonly StyledProperty<object?> LeadingContentProperty =
        AvaloniaProperty.Register<ChatHeader, object?>(nameof(LeadingContent));

    public static readonly StyledProperty<IDataTemplate?> LeadingContentTemplateProperty =
        AvaloniaProperty.Register<ChatHeader, IDataTemplate?>(nameof(LeadingContentTemplate));

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<ChatHeader, object?>(nameof(ActionContent));

    public static readonly StyledProperty<IDataTemplate?> ActionContentTemplateProperty =
        AvaloniaProperty.Register<ChatHeader, IDataTemplate?>(nameof(ActionContentTemplate));

    public static readonly DirectProperty<ChatHeader, bool> HasContentProperty =
        AvaloniaProperty.RegisterDirect<ChatHeader, bool>(nameof(HasContent), header => header.HasContent);

    private bool _hasContent;

    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string? Status { get => GetValue(StatusProperty); set => SetValue(StatusProperty, value); }
    public BoxShadows BoxShadow { get => GetValue(BoxShadowProperty); set => SetValue(BoxShadowProperty, value); }
    public object? LeadingContent { get => GetValue(LeadingContentProperty); set => SetValue(LeadingContentProperty, value); }
    public IDataTemplate? LeadingContentTemplate { get => GetValue(LeadingContentTemplateProperty); set => SetValue(LeadingContentTemplateProperty, value); }
    public object? ActionContent { get => GetValue(ActionContentProperty); set => SetValue(ActionContentProperty, value); }
    public IDataTemplate? ActionContentTemplate { get => GetValue(ActionContentTemplateProperty); set => SetValue(ActionContentTemplateProperty, value); }
    public bool HasContent { get => _hasContent; private set => SetAndRaise(HasContentProperty, ref _hasContent, value); }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TitleProperty
            || change.Property == StatusProperty
            || change.Property == LeadingContentProperty
            || change.Property == ActionContentProperty)
        {
            HasContent = !string.IsNullOrEmpty(Title)
                || !string.IsNullOrEmpty(Status)
                || LeadingContent is not null
                || ActionContent is not null;
        }
    }
}