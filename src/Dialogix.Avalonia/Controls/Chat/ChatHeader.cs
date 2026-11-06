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

    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string? Status { get => GetValue(StatusProperty); set => SetValue(StatusProperty, value); }
    public BoxShadows BoxShadow { get => GetValue(BoxShadowProperty); set => SetValue(BoxShadowProperty, value); }
}