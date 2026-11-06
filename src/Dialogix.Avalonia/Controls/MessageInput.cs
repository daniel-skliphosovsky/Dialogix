using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.VisualTree;

namespace Dialogix.Avalonia.Controls;

public class MessageInput : TemplatedControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<MessageInput, string?>(
            nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> WatermarkProperty =
        AvaloniaProperty.Register<MessageInput, string?>(nameof(Watermark), "Message");

    public static readonly StyledProperty<int> MaxLinesProperty =
        AvaloniaProperty.Register<MessageInput, int>(nameof(MaxLines), 5);

    public static readonly StyledProperty<int> MaxLengthProperty =
        AvaloniaProperty.Register<MessageInput, int>(nameof(MaxLength));

    public static readonly StyledProperty<int> CharacterCountProperty =
        AvaloniaProperty.Register<MessageInput, int>(nameof(CharacterCount));

    public static readonly StyledProperty<bool> IsTextTooLongProperty =
        AvaloniaProperty.Register<MessageInput, bool>(nameof(IsTextTooLong));

    public static readonly StyledProperty<bool> CanSendProperty =
        AvaloniaProperty.Register<MessageInput, bool>(nameof(CanSend));

    public static readonly StyledProperty<ICommand?> SendCommandProperty =
        AvaloniaProperty.Register<MessageInput, ICommand?>(nameof(SendCommand));

    public static readonly StyledProperty<object?> SendIconProperty =
        AvaloniaProperty.Register<MessageInput, object?>(nameof(SendIcon));

    private TextBox? _textBox;
    private Button? _sendButton;
    private TextBlock? _counter;
    private double _lastWidth = -1;

    static MessageInput()
    {
        MaxLinesProperty.Changed.AddClassHandler<MessageInput>((input, _) => input.UpdateTextHeight());
        MaxLengthProperty.Changed.AddClassHandler<MessageInput>((input, _) => input.UpdateTextState());
        TextProperty.Changed.AddClassHandler<MessageInput>((input, _) => input.UpdateTextState());
    }

    public event EventHandler? Sent;

    public string? Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string? Watermark { get => GetValue(WatermarkProperty); set => SetValue(WatermarkProperty, value); }
    public int MaxLines { get => GetValue(MaxLinesProperty); set => SetValue(MaxLinesProperty, value); }
    public int MaxLength { get => GetValue(MaxLengthProperty); set => SetValue(MaxLengthProperty, value); }
    public int CharacterCount { get => GetValue(CharacterCountProperty); private set => SetValue(CharacterCountProperty, value); }
    public bool IsTextTooLong { get => GetValue(IsTextTooLongProperty); private set => SetValue(IsTextTooLongProperty, value); }
    public bool CanSend { get => GetValue(CanSendProperty); private set => SetValue(CanSendProperty, value); }
    public ICommand? SendCommand { get => GetValue(SendCommandProperty); set => SetValue(SendCommandProperty, value); }
    public object? SendIcon { get => GetValue(SendIconProperty); set => SetValue(SendIconProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_textBox is not null)
        {
            _textBox.RemoveHandler(KeyDownEvent, OnPreviewKeyDown);
            _textBox.GotFocus -= OnTextBoxGotFocus;
            _textBox.LostFocus -= OnTextBoxLostFocus;
            _textBox.PropertyChanged -= OnTextBoxPropertyChanged;
        }

        if (_sendButton is not null)
            _sendButton.Click -= OnSendButtonClick;

        base.OnApplyTemplate(e);

        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        _sendButton = e.NameScope.Find<Button>("PART_SendButton");
        _counter = e.NameScope.Find<TextBlock>("PART_Counter");

        if (_sendButton is not null)
            _sendButton.Click += OnSendButtonClick;

        if (_textBox is not null)
        {
            _textBox.AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
            _textBox.GotFocus += OnTextBoxGotFocus;
            _textBox.LostFocus += OnTextBoxLostFocus;
            _textBox.PropertyChanged += OnTextBoxPropertyChanged;
            _lastWidth = -1;

            UpdateTextState();
        }
    }

    private void UpdateTextState()
    {
        string text = Text ?? string.Empty;

        CharacterCount = text.Length;
        IsTextTooLong = MaxLength > 0 && CharacterCount > MaxLength;
        CanSend = !string.IsNullOrWhiteSpace(text) && !IsTextTooLong;
        PseudoClasses.Set(":toolong", IsTextTooLong);

        UpdateCounter();
        UpdateTextHeight();
    }

    private void UpdateCounter()
    {
        if (_counter is null)
            return;

        _counter.IsVisible = MaxLength > 0;

        if (MaxLength > 0)
            _counter.Text = $"{CharacterCount}/{MaxLength}";
    }

    // FluentAvalonia's TextBox template has no PART_ScrollViewer, so TextBox.MaxLines is ignored
    // there and the control grows without a limit. Cap the height ourselves, and only as tall as
    // the current text needs so an empty input stays one line.
    private void UpdateTextHeight()
    {
        if (_textBox is null)
            return;

        if (MaxLines <= 0)
        {
            _textBox.ClearValue(MaxHeightProperty);
            return;
        }

        Typeface typeface = new(_textBox.FontFamily, _textBox.FontStyle, _textBox.FontWeight, _textBox.FontStretch);
        double lineHeight = LayoutHeight("A", typeface, double.PositiveInfinity);
        double padding = _textBox.Padding.Top + _textBox.Padding.Bottom;
        double height = lineHeight;

        string? text = _textBox.Text;
        double contentWidth = _textBox.Bounds.Width - _textBox.Padding.Left - _textBox.Padding.Right;

        if (!string.IsNullOrWhiteSpace(text) && contentWidth > 0)
            height = Math.Min(LayoutHeight(text, typeface, contentWidth), lineHeight * MaxLines);

        _textBox.MaxHeight = Math.Ceiling(Math.Max(lineHeight, height)) + padding;
    }

    private double LayoutHeight(string text, Typeface typeface, double maxWidth)
    {
        if (_textBox is null)
            return 0;

        TextLayout layout = new(
            text,
            typeface,
            _textBox.FontSize,
            maxWidth: maxWidth,
            textWrapping: TextWrapping.Wrap,
            lineHeight: _textBox.LineHeight,
            letterSpacing: _textBox.LetterSpacing);

        return layout.Height;
    }

    private void OnTextBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty)
        {
            double width = _textBox?.Bounds.Width ?? 0;

            if (Math.Abs(width - _lastWidth) < 0.5)
                return;

            _lastWidth = width;
            UpdateTextHeight();
            return;
        }

        if (e.Property == FontFamilyProperty
            || e.Property == FontSizeProperty
            || e.Property == FontStyleProperty
            || e.Property == FontWeightProperty
            || e.Property == FontStretchProperty
            || e.Property == LetterSpacingProperty
            || e.Property == TextBox.LineHeightProperty)
        {
            UpdateTextHeight();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (_textBox is null || e.Handled)
            return;

        if (e.Source is Visual source && source.GetSelfAndVisualAncestors().OfType<Button>().Any())
            return;

        if (!_textBox.IsKeyboardFocusWithin)
            _textBox.Focus();
    }

    private void OnSendButtonClick(object? sender, RoutedEventArgs e) => Send();

    private void OnTextBoxGotFocus(object? sender, RoutedEventArgs e) => PseudoClasses.Set(":focused", true);

    private void OnTextBoxLostFocus(object? sender, RoutedEventArgs e) => PseudoClasses.Set(":focused", false);

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            return;

        e.Handled = true;
        Send();
    }

    private void Send()
    {
        string? text = Text;

        if (string.IsNullOrWhiteSpace(text) || IsTextTooLong)
            return;

        ICommand? command = SendCommand;

        if (command is null || !command.CanExecute(text))
            return;

        command.Execute(text);
        Text = string.Empty;

        _textBox?.Text = string.Empty;

        Sent?.Invoke(this, EventArgs.Empty);
    }
}