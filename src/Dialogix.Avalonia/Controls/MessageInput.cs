using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

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

    public static readonly StyledProperty<ICommand?> SendCommandProperty =
        AvaloniaProperty.Register<MessageInput, ICommand?>(nameof(SendCommand));

    public static readonly StyledProperty<Geometry?> SendIconProperty =
        AvaloniaProperty.Register<MessageInput, Geometry?>(nameof(SendIcon));

    private TextBox? _textBox;
    private Button? _sendButton;

    public event EventHandler? Sent;

    public string? Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string? Watermark { get => GetValue(WatermarkProperty); set => SetValue(WatermarkProperty, value); }
    public int MaxLines { get => GetValue(MaxLinesProperty); set => SetValue(MaxLinesProperty, value); }
    public ICommand? SendCommand { get => GetValue(SendCommandProperty); set => SetValue(SendCommandProperty, value); }
    public Geometry? SendIcon { get => GetValue(SendIconProperty); set => SetValue(SendIconProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_textBox is not null)
        {
            _textBox.RemoveHandler(KeyDownEvent, OnPreviewKeyDown);
            _textBox.GotFocus -= OnTextBoxGotFocus;
            _textBox.LostFocus -= OnTextBoxLostFocus;
        }

        if (_sendButton is not null)
            _sendButton.Click -= OnSendButtonClick;

        base.OnApplyTemplate(e);

        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        _sendButton = e.NameScope.Find<Button>("PART_SendButton");

        if (_sendButton is not null)
            _sendButton.Click += OnSendButtonClick;

        if (_textBox is not null)
        {
            _textBox.AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
            _textBox.GotFocus += OnTextBoxGotFocus;
            _textBox.LostFocus += OnTextBoxLostFocus;
        }
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

        if (string.IsNullOrWhiteSpace(text))
            return;

        ICommand? command = SendCommand;

        if (command is null || !command.CanExecute(text))
            return;

        command.Execute(text);
        Text = string.Empty;

        if (_textBox is not null)
            _textBox.Text = string.Empty;

        Sent?.Invoke(this, EventArgs.Empty);
    }
}