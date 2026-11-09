using System.Collections;
using System.Collections.Specialized;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace Dialogix.Avalonia.Controls;

/// <summary>
/// A scrollable, virtualized list of conversations.
/// </summary>
public class ConversationListView : TemplatedControl
{
    public static readonly StyledProperty<IEnumerable?> ItemsProperty =
        AvaloniaProperty.Register<ConversationListView, IEnumerable?>(nameof(Items));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<ConversationListView, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<object?> SelectedConversationProperty =
        AvaloniaProperty.Register<ConversationListView, object?>(
            nameof(SelectedConversation),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object?> EmptyContentProperty =
        AvaloniaProperty.Register<ConversationListView, object?>(nameof(EmptyContent));

    public static readonly StyledProperty<IDataTemplate?> EmptyContentTemplateProperty =
        AvaloniaProperty.Register<ConversationListView, IDataTemplate?>(nameof(EmptyContentTemplate));

    public static readonly DirectProperty<ConversationListView, bool> IsEmptyProperty =
        AvaloniaProperty.RegisterDirect<ConversationListView, bool>(
            nameof(IsEmpty),
            view => view.IsEmpty);

    private INotifyCollectionChanged? _notifyItems;
    private bool _isEmpty = true;

    public IEnumerable? Items { get => GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }
    public IDataTemplate? ItemTemplate { get => GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }
    public object? SelectedConversation { get => GetValue(SelectedConversationProperty); set => SetValue(SelectedConversationProperty, value); }
    public object? EmptyContent { get => GetValue(EmptyContentProperty); set => SetValue(EmptyContentProperty, value); }
    public IDataTemplate? EmptyContentTemplate { get => GetValue(EmptyContentTemplateProperty); set => SetValue(EmptyContentTemplateProperty, value); }
    public bool IsEmpty { get => _isEmpty; private set => SetAndRaise(IsEmptyProperty, ref _isEmpty, value); }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != ItemsProperty)
            return;

        if (_notifyItems is not null)
            _notifyItems.CollectionChanged -= OnItemsChanged;

        _notifyItems = change.GetNewValue<IEnumerable?>() as INotifyCollectionChanged;

        if (_notifyItems is not null)
            _notifyItems.CollectionChanged += OnItemsChanged;

        IsEmpty = IsCollectionEmpty(change.GetNewValue<IEnumerable?>());
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        IsEmpty = IsCollectionEmpty(Items);

    private static bool IsCollectionEmpty(IEnumerable? items)
    {
        if (items is null)
            return true;

        if (items is ICollection collection)
            return collection.Count == 0;

        IEnumerator enumerator = items.GetEnumerator();

        try
        {
            return !enumerator.MoveNext();
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }
}