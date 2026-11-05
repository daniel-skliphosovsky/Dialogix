using System.Collections;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Dialogix.Avalonia.Controls;

public class ProfileView : TemplatedControl
{
    public static readonly StyledProperty<IImage?> AvatarProperty =
        AvaloniaProperty.Register<ProfileView, IImage?>(nameof(Avatar));

    public static readonly StyledProperty<object?> PlaceholderIconProperty =
        AvaloniaProperty.Register<ProfileView, object?>(nameof(PlaceholderIcon));

    public static readonly StyledProperty<string?> DisplayNameProperty =
        AvaloniaProperty.Register<ProfileView, string?>(nameof(DisplayName));

    public static readonly StyledProperty<IEnumerable?> ItemsProperty =
        AvaloniaProperty.Register<ProfileView, IEnumerable?>(nameof(Items));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<ProfileView, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<ICommand?> EditCommandProperty =
        AvaloniaProperty.Register<ProfileView, ICommand?>(nameof(EditCommand));

    public static readonly StyledProperty<object?> EditIconProperty =
        AvaloniaProperty.Register<ProfileView, object?>(nameof(EditIcon));

    public static readonly StyledProperty<bool> IsEditVisibleProperty =
        AvaloniaProperty.Register<ProfileView, bool>(nameof(IsEditVisible), true);

    public static readonly StyledProperty<ICommand?> ItemCommandProperty =
        AvaloniaProperty.Register<ProfileView, ICommand?>(nameof(ItemCommand));

    public IImage? Avatar { get => GetValue(AvatarProperty); set => SetValue(AvatarProperty, value); }
    public object? PlaceholderIcon { get => GetValue(PlaceholderIconProperty); set => SetValue(PlaceholderIconProperty, value); }
    public string? DisplayName { get => GetValue(DisplayNameProperty); set => SetValue(DisplayNameProperty, value); }
    public IEnumerable? Items { get => GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }
    public IDataTemplate? ItemTemplate { get => GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }
    public ICommand? EditCommand { get => GetValue(EditCommandProperty); set => SetValue(EditCommandProperty, value); }
    public object? EditIcon { get => GetValue(EditIconProperty); set => SetValue(EditIconProperty, value); }
    public bool IsEditVisible { get => GetValue(IsEditVisibleProperty); set => SetValue(IsEditVisibleProperty, value); }
    public ICommand? ItemCommand { get => GetValue(ItemCommandProperty); set => SetValue(ItemCommandProperty, value); }

    private ItemsControl? _items;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _items = e.NameScope.Find<ItemsControl>("PART_Items");
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (e.Handled || e.InitialPressMouseButton != MouseButton.Left)
            return;

        if (ItemCommand is not { } command || FindItem(e.Source as Visual) is not { } item)
            return;

        if (!command.CanExecute(item))
            return;

        e.Handled = true;
        command.Execute(item);
    }

    private object? FindItem(Visual? source)
    {
        if (_items is null || Items is null)
            return null;

        for (Visual? visual = source; visual is not null && visual != _items; visual = visual.GetVisualParent())
        {
            if (visual is Control { DataContext: { } data } && Contains(data))
                return data;
        }

        return null;
    }

    private bool Contains(object data)
    {
        foreach (object? item in Items!)
        {
            if (Equals(item, data))
                return true;
        }

        return false;
    }
}