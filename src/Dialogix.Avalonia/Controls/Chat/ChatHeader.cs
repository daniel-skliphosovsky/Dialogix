using Avalonia;
using Avalonia.Controls.Primitives;
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

    public static readonly DirectProperty<ChatHeader, bool> HasContentProperty =
        AvaloniaProperty.RegisterDirect<ChatHeader, bool>(nameof(HasContent), header => header.HasContent);

    private bool _hasContent;

    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string? Status { get => GetValue(StatusProperty); set => SetValue(StatusProperty, value); }
    public BoxShadows BoxShadow { get => GetValue(BoxShadowProperty); set => SetValue(BoxShadowProperty, value); }
    public bool HasContent { get => _hasContent; private set => SetAndRaise(HasContentProperty, ref _hasContent, value); }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TitleProperty || change.Property == StatusProperty)
            HasContent = !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(Status);
    }
}