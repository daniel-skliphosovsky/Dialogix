using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Dialogix.Avalonia.Controls;

public class MessageBubble : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<MessageBubble, string?>(nameof(Text));

    public static readonly StyledProperty<string?> AuthorProperty =
        AvaloniaProperty.Register<MessageBubble, string?>(nameof(Author));

    public static readonly StyledProperty<string?> TimestampProperty =
        AvaloniaProperty.Register<MessageBubble, string?>(nameof(Timestamp));

    public static readonly StyledProperty<bool> IsOutgoingProperty =
        AvaloniaProperty.Register<MessageBubble, bool>(nameof(IsOutgoing));

    public static readonly StyledProperty<double> MinBubbleWidthProperty =
        AvaloniaProperty.Register<MessageBubble, double>(nameof(MinBubbleWidth));

    public static readonly StyledProperty<double> MaxBubbleWidthProperty =
        AvaloniaProperty.Register<MessageBubble, double>(nameof(MaxBubbleWidth), 520d);

    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        AvaloniaProperty.Register<MessageBubble, BoxShadows>(nameof(BoxShadow));

    public string? Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string? Author { get => GetValue(AuthorProperty); set => SetValue(AuthorProperty, value); }
    public string? Timestamp { get => GetValue(TimestampProperty); set => SetValue(TimestampProperty, value); }
    public bool IsOutgoing { get => GetValue(IsOutgoingProperty); set => SetValue(IsOutgoingProperty, value); }
    public double MinBubbleWidth { get => GetValue(MinBubbleWidthProperty); set => SetValue(MinBubbleWidthProperty, value); }
    public double MaxBubbleWidth { get => GetValue(MaxBubbleWidthProperty); set => SetValue(MaxBubbleWidthProperty, value); }
    public BoxShadows BoxShadow { get => GetValue(BoxShadowProperty); set => SetValue(BoxShadowProperty, value); }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsOutgoingProperty)
            PseudoClasses.Set(":outgoing", change.GetNewValue<bool>());
    }
}