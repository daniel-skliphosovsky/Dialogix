using System.Windows.Input;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;

namespace Dialogix.Avalonia.Controls;

public class ProfileEditorView : TemplatedControl
{
    public static readonly StyledProperty<IImage?> AvatarProperty =
        AvaloniaProperty.Register<ProfileEditorView, IImage?>(nameof(Avatar));

    public static readonly StyledProperty<object?> PlaceholderIconProperty =
        AvaloniaProperty.Register<ProfileEditorView, object?>(nameof(PlaceholderIcon));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ProfileEditorView, string?>(nameof(Title), "Edit profile");

    public static readonly StyledProperty<object?> FieldsProperty =
        AvaloniaProperty.Register<ProfileEditorView, object?>(nameof(Fields));

    public static readonly StyledProperty<IDataTemplate?> FieldsTemplateProperty =
        AvaloniaProperty.Register<ProfileEditorView, IDataTemplate?>(nameof(FieldsTemplate));

    public static readonly StyledProperty<ICommand?> SaveCommandProperty =
        AvaloniaProperty.Register<ProfileEditorView, ICommand?>(nameof(SaveCommand));

    public static readonly StyledProperty<ICommand?> CancelCommandProperty =
        AvaloniaProperty.Register<ProfileEditorView, ICommand?>(nameof(CancelCommand));

    public static readonly StyledProperty<string?> SaveTextProperty =
        AvaloniaProperty.Register<ProfileEditorView, string?>(nameof(SaveText), "Save");

    public static readonly StyledProperty<string?> CancelTextProperty =
        AvaloniaProperty.Register<ProfileEditorView, string?>(nameof(CancelText), "Cancel");

    public static readonly StyledProperty<bool> IsSaveOnEnterEnabledProperty =
        AvaloniaProperty.Register<ProfileEditorView, bool>(nameof(IsSaveOnEnterEnabled), true);

    public static readonly StyledProperty<bool> IsCancelOnEscapeEnabledProperty =
        AvaloniaProperty.Register<ProfileEditorView, bool>(nameof(IsCancelOnEscapeEnabled), true);

    public IImage? Avatar { get => GetValue(AvatarProperty); set => SetValue(AvatarProperty, value); }
    public object? PlaceholderIcon { get => GetValue(PlaceholderIconProperty); set => SetValue(PlaceholderIconProperty, value); }
    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public object? Fields { get => GetValue(FieldsProperty); set => SetValue(FieldsProperty, value); }
    public IDataTemplate? FieldsTemplate { get => GetValue(FieldsTemplateProperty); set => SetValue(FieldsTemplateProperty, value); }
    public ICommand? SaveCommand { get => GetValue(SaveCommandProperty); set => SetValue(SaveCommandProperty, value); }
    public ICommand? CancelCommand { get => GetValue(CancelCommandProperty); set => SetValue(CancelCommandProperty, value); }
    public string? SaveText { get => GetValue(SaveTextProperty); set => SetValue(SaveTextProperty, value); }
    public string? CancelText { get => GetValue(CancelTextProperty); set => SetValue(CancelTextProperty, value); }
    /// <summary>Whether Enter runs <see cref="SaveCommand"/>.</summary>
    public bool IsSaveOnEnterEnabled { get => GetValue(IsSaveOnEnterEnabledProperty); set => SetValue(IsSaveOnEnterEnabledProperty, value); }
    /// <summary>Whether Escape runs <see cref="CancelCommand"/>.</summary>
    public bool IsCancelOnEscapeEnabled { get => GetValue(IsCancelOnEscapeEnabledProperty); set => SetValue(IsCancelOnEscapeEnabledProperty, value); }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
            return;

        ICommand? command = e.Key switch
        {
            Key.Enter when IsSaveOnEnterEnabled => SaveCommand,
            Key.Escape when IsCancelOnEscapeEnabled => CancelCommand,
            _ => null
        };

        if (command is null || !command.CanExecute(null))
            return;

        e.Handled = true;
        command.Execute(null);
    }
}