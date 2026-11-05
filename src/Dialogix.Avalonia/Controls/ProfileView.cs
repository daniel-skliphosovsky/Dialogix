using System.Collections;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace Dialogix.Avalonia.Controls;

public class ProfileView : TemplatedControl
{
    public static readonly StyledProperty<IImage?> AvatarProperty =
        AvaloniaProperty.Register<ProfileView, IImage?>(nameof(Avatar));

    public static readonly StyledProperty<Geometry?> PlaceholderIconProperty =
        AvaloniaProperty.Register<ProfileView, Geometry?>(nameof(PlaceholderIcon));

    public static readonly StyledProperty<string?> DisplayNameProperty =
        AvaloniaProperty.Register<ProfileView, string?>(nameof(DisplayName));

    public static readonly StyledProperty<IEnumerable?> ItemsProperty =
        AvaloniaProperty.Register<ProfileView, IEnumerable?>(nameof(Items));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<ProfileView, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<ICommand?> EditCommandProperty =
        AvaloniaProperty.Register<ProfileView, ICommand?>(nameof(EditCommand));

    public static readonly StyledProperty<Geometry?> EditIconProperty =
        AvaloniaProperty.Register<ProfileView, Geometry?>(nameof(EditIcon));

    public static readonly StyledProperty<bool> IsEditVisibleProperty =
        AvaloniaProperty.Register<ProfileView, bool>(nameof(IsEditVisible), true);

    public IImage? Avatar { get => GetValue(AvatarProperty); set => SetValue(AvatarProperty, value); }
    public Geometry? PlaceholderIcon { get => GetValue(PlaceholderIconProperty); set => SetValue(PlaceholderIconProperty, value); }
    public string? DisplayName { get => GetValue(DisplayNameProperty); set => SetValue(DisplayNameProperty, value); }
    public IEnumerable? Items { get => GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }
    public IDataTemplate? ItemTemplate { get => GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }
    public ICommand? EditCommand { get => GetValue(EditCommandProperty); set => SetValue(EditCommandProperty, value); }
    public Geometry? EditIcon { get => GetValue(EditIconProperty); set => SetValue(EditIconProperty, value); }
    public bool IsEditVisible { get => GetValue(IsEditVisibleProperty); set => SetValue(IsEditVisibleProperty, value); }
}