using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Dialogix.Avalonia.Controls;

public class ChatView : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> MessagesProperty =
        AvaloniaProperty.Register<ChatView, IEnumerable?>(nameof(Messages));

    public static readonly StyledProperty<IDataTemplate?> MessageTemplateProperty =
        AvaloniaProperty.Register<ChatView, IDataTemplate?>(nameof(MessageTemplate));

    public static readonly StyledProperty<bool> HasMoreProperty =
        AvaloniaProperty.Register<ChatView, bool>(nameof(HasMore));

    public static readonly StyledProperty<bool> IsLoadingMoreProperty =
        AvaloniaProperty.Register<ChatView, bool>(nameof(IsLoadingMore));

    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<ChatView, object?>(nameof(LoadingContent));

    public static readonly StyledProperty<IDataTemplate?> LoadingContentTemplateProperty =
        AvaloniaProperty.Register<ChatView, IDataTemplate?>(nameof(LoadingContentTemplate));

    public static readonly StyledProperty<ICommand?> LoadMoreCommandProperty =
        AvaloniaProperty.Register<ChatView, ICommand?>(nameof(LoadMoreCommand));

    public static readonly StyledProperty<double> LoadMoreThresholdProperty =
        AvaloniaProperty.Register<ChatView, double>(nameof(LoadMoreThreshold), 48d);

    public static readonly StyledProperty<object?> ScrollToEndIconProperty =
        AvaloniaProperty.Register<ChatView, object?>(nameof(ScrollToEndIcon));

    public static readonly StyledProperty<bool> SmoothScrollingProperty =
        AvaloniaProperty.Register<ChatView, bool>(nameof(SmoothScrolling), true);

    private ScrollViewer? _scroll;
    private Button? _scrollToEnd;
    private INotifyCollectionChanged? _notifyMessages;
    private bool _canLoadMore = true;
    private double _anchorExtent;
    private double _anchorOffset;
    private bool _stickToBottom = true;
    private bool _initialScrollPending = true;
    private DispatcherTimer? _scrollTimer;

    public IEnumerable? Messages { get => GetValue(MessagesProperty); set => SetValue(MessagesProperty, value); }
    public IDataTemplate? MessageTemplate { get => GetValue(MessageTemplateProperty); set => SetValue(MessageTemplateProperty, value); }
    public bool HasMore { get => GetValue(HasMoreProperty); set => SetValue(HasMoreProperty, value); }
    public bool IsLoadingMore { get => GetValue(IsLoadingMoreProperty); set => SetValue(IsLoadingMoreProperty, value); }
    public object? LoadingContent { get => GetValue(LoadingContentProperty); set => SetValue(LoadingContentProperty, value); }
    public IDataTemplate? LoadingContentTemplate { get => GetValue(LoadingContentTemplateProperty); set => SetValue(LoadingContentTemplateProperty, value); }
    public ICommand? LoadMoreCommand { get => GetValue(LoadMoreCommandProperty); set => SetValue(LoadMoreCommandProperty, value); }
    public double LoadMoreThreshold { get => GetValue(LoadMoreThresholdProperty); set => SetValue(LoadMoreThresholdProperty, value); }
    public object? ScrollToEndIcon { get => GetValue(ScrollToEndIconProperty); set => SetValue(ScrollToEndIconProperty, value); }
    public bool SmoothScrolling { get => GetValue(SmoothScrollingProperty); set => SetValue(SmoothScrollingProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _scrollTimer?.Stop();
        _scrollTimer = null;

        if (_scroll is not null)
            _scroll.ScrollChanged -= OnScrollChanged;

        if (_scrollToEnd is not null)
            _scrollToEnd.Click -= OnScrollToEndClick;

        base.OnApplyTemplate(e);

        _scroll = e.NameScope.Find<ScrollViewer>("PART_Scroll");
        _scrollToEnd = e.NameScope.Find<Button>("PART_ScrollToEnd");

        if (_scroll is not null)
            _scroll.ScrollChanged += OnScrollChanged;

        if (_scrollToEnd is not null)
            _scrollToEnd.Click += OnScrollToEndClick;

    }

    public void ScrollToEnd()
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

            _canLoadMore = true;
        }
        else if (change.Property == HasMoreProperty && change.GetNewValue<bool>())
        {
            Dispatcher.UIThread.Post(TryLoadMore, DispatcherPriority.Loaded);
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

            if (_scroll.Extent.Height <= _scroll.Viewport.Height + 0.5)
                TryLoadMore();

            return;
        }

        _stickToBottom = _scroll.Offset.Y >= _scroll.Extent.Height - _scroll.Viewport.Height - 1;
        _scrollToEnd?.SetCurrentValue(IsVisibleProperty, !_stickToBottom && _scroll.Extent.Height > _scroll.Viewport.Height);

        if (_scroll.Offset.Y > LoadMoreThreshold)
        {
            _canLoadMore = true;
            return;
        }

        if (_scroll.Extent.Height <= _scroll.Viewport.Height + 0.5)
            _canLoadMore = true;

        TryLoadMore();
    }

    private void TryLoadMore()
    {
        if (_scroll is null || !_canLoadMore || !HasMore || IsLoadingMore)
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