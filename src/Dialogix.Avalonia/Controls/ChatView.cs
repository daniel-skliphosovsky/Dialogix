using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace Dialogix.Avalonia.Controls;

public class ChatView : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> MessagesProperty =
        AvaloniaProperty.Register<ChatView, IEnumerable?>(nameof(Messages));

    public static readonly StyledProperty<bool> HasMoreProperty =
        AvaloniaProperty.Register<ChatView, bool>(nameof(HasMore));

    public static readonly StyledProperty<bool> IsLoadingMoreProperty =
        AvaloniaProperty.Register<ChatView, bool>(nameof(IsLoadingMore));

    public static readonly StyledProperty<ICommand?> LoadMoreCommandProperty =
        AvaloniaProperty.Register<ChatView, ICommand?>(nameof(LoadMoreCommand));

    public static readonly StyledProperty<double> LoadMoreThresholdProperty =
        AvaloniaProperty.Register<ChatView, double>(nameof(LoadMoreThreshold), 48d);

    public static readonly StyledProperty<string?> InputTextProperty =
        AvaloniaProperty.Register<ChatView, string?>(
            nameof(InputText), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> InputWatermarkProperty =
        AvaloniaProperty.Register<ChatView, string?>(nameof(InputWatermark), "Message");

    public static readonly StyledProperty<ICommand?> SendCommandProperty =
        AvaloniaProperty.Register<ChatView, ICommand?>(nameof(SendCommand));

    public static readonly StyledProperty<Geometry?> ScrollToEndIconProperty =
        AvaloniaProperty.Register<ChatView, Geometry?>(nameof(ScrollToEndIcon));

    public static readonly StyledProperty<bool> SmoothScrollingProperty =
        AvaloniaProperty.Register<ChatView, bool>(nameof(SmoothScrolling), true);

    private ScrollViewer? _scroll;
    private Button? _scrollToEnd;
    private MessageInput? _input;
    private INotifyCollectionChanged? _notifyMessages;
    private bool _canLoadMore = true;
    private double _anchorExtent;
    private double _anchorOffset;
    private bool _stickToBottom = true;
    private bool _initialScrollPending = true;
    private DispatcherTimer? _scrollTimer;

    public IEnumerable? Messages { get => GetValue(MessagesProperty); set => SetValue(MessagesProperty, value); }
    public bool HasMore { get => GetValue(HasMoreProperty); set => SetValue(HasMoreProperty, value); }
    public bool IsLoadingMore { get => GetValue(IsLoadingMoreProperty); set => SetValue(IsLoadingMoreProperty, value); }
    public ICommand? LoadMoreCommand { get => GetValue(LoadMoreCommandProperty); set => SetValue(LoadMoreCommandProperty, value); }
    public double LoadMoreThreshold { get => GetValue(LoadMoreThresholdProperty); set => SetValue(LoadMoreThresholdProperty, value); }
    public string? InputText { get => GetValue(InputTextProperty); set => SetValue(InputTextProperty, value); }
    public string? InputWatermark { get => GetValue(InputWatermarkProperty); set => SetValue(InputWatermarkProperty, value); }
    public ICommand? SendCommand { get => GetValue(SendCommandProperty); set => SetValue(SendCommandProperty, value); }
    public Geometry? ScrollToEndIcon { get => GetValue(ScrollToEndIconProperty); set => SetValue(ScrollToEndIconProperty, value); }
    public bool SmoothScrolling { get => GetValue(SmoothScrollingProperty); set => SetValue(SmoothScrollingProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _scrollTimer?.Stop();
        _scrollTimer = null;

        if (_scroll is not null)
            _scroll.ScrollChanged -= OnScrollChanged;

        if (_scrollToEnd is not null)
            _scrollToEnd.Click -= OnScrollToEndClick;

        if (_input is not null)
            _input.Sent -= OnInputSent;

        base.OnApplyTemplate(e);

        _scroll = e.NameScope.Find<ScrollViewer>("PART_Scroll");
        _scrollToEnd = e.NameScope.Find<Button>("PART_ScrollToEnd");
        _input = e.NameScope.Find<MessageInput>("PART_Input");

        if (_scroll is not null)
            _scroll.ScrollChanged += OnScrollChanged;

        if (_scrollToEnd is not null)
            _scrollToEnd.Click += OnScrollToEndClick;

        if (_input is not null)
            _input.Sent += OnInputSent;
    }

    private void OnInputSent(object? sender, EventArgs e)
    {
        _stickToBottom = true;
        _scrollTimer?.Stop();
        _scrollTimer = null;
        ScrollToEndDeferred();
    }

    private void ScrollToEndDeferred()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_scroll is null)
                return;

            _scroll.UpdateLayout();
            _scroll.ScrollToEnd();
        }, DispatcherPriority.Loaded);
    }

    private void OnScrollToEndClick(object? sender, RoutedEventArgs e)
    {
        if (SmoothScrolling)
            ScrollToEndSmoothly();
        else
            _scroll?.ScrollToEnd();
    }

    private void ScrollToEndSmoothly()
    {
        ScrollViewer? scroll = _scroll;

        if (scroll is null)
            return;

        const double duration = 280d;
        double start = scroll.Offset.Y;
        double target = Math.Max(0d, scroll.Extent.Height - scroll.Viewport.Height);

        if (Math.Abs(target - start) < 0.5)
            return;

        _scrollTimer?.Stop();

        Stopwatch elapsed = Stopwatch.StartNew();
        DispatcherTimer timer = new(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, Dispatcher.UIThread);

        timer.Tick += (_, _) =>
        {
            double progress = Math.Min(1d, elapsed.Elapsed.TotalMilliseconds / duration);
            double eased = 1d - Math.Pow(1d - progress, 3d);

            scroll.Offset = new Vector(scroll.Offset.X, start + ((target - start) * eased));

            if (progress < 1d)
                return;

            timer.Stop();

            if (ReferenceEquals(_scrollTimer, timer))
                _scrollTimer = null;
        };

        _scrollTimer = timer;
        timer.Start();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MessagesProperty)
        {
            if (_notifyMessages is not null)
                _notifyMessages.CollectionChanged -= OnMessagesChanged;

            _notifyMessages = change.GetNewValue<IEnumerable?>() as INotifyCollectionChanged;

            if (_notifyMessages is not null)
                _notifyMessages.CollectionChanged += OnMessagesChanged;
        }
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_scroll is null)
            return;

        if (_initialScrollPending && _scroll.Extent.Height > 0)
        {
            _initialScrollPending = false;
            _scroll.ScrollToEnd();
            return;
        }

        _stickToBottom = _scroll.Offset.Y >= _scroll.Extent.Height - _scroll.Viewport.Height - 1;
        _scrollToEnd?.SetCurrentValue(IsVisibleProperty, !_stickToBottom && _scroll.Extent.Height > _scroll.Viewport.Height);

        if (_scroll.Offset.Y > LoadMoreThreshold)
        {
            _canLoadMore = true;
            return;
        }

        if (!_canLoadMore || !HasMore || IsLoadingMore)
            return;

        ICommand? command = LoadMoreCommand;

        if (command is null || !command.CanExecute(null))
            return;

        _canLoadMore = false;
        _anchorExtent = _scroll.Extent.Height;
        _anchorOffset = _scroll.Offset.Y;
        command.Execute(null);
    }

    private void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        bool prepended = e is { Action: NotifyCollectionChangedAction.Add, NewStartingIndex: 0 };

        if (prepended && _anchorExtent > 0)
        {
            double extent = _anchorExtent;
            double offset = _anchorOffset;
            _anchorExtent = 0;

            Dispatcher.UIThread.Post(() =>
            {
                if (_scroll is null)
                    return;

                double delta = _scroll.Extent.Height - extent;

                if (delta > 0)
                    _scroll.Offset = _scroll.Offset.WithY(offset + delta);
            }, DispatcherPriority.Loaded);
            return;
        }

        if (!_stickToBottom)
            return;

        ScrollToEndDeferred();
    }
}