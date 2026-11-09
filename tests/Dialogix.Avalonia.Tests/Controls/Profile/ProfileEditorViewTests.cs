using System.Windows.Input;

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Dialogix.Avalonia.Controls;


namespace Dialogix.Avalonia.Tests.Controls.Profile;

public class ProfileEditorViewTests
{
    [AvaloniaFact]
    public void Defaults_are_expected()
    {
        ProfileEditorView editor = new ProfileEditorView();

        Assert.Null(editor.Avatar);
        Assert.Null(editor.PlaceholderIcon);
        Assert.Equal("Edit profile", editor.Title);
        Assert.Null(editor.Fields);
        Assert.Null(editor.FieldsTemplate);
        Assert.Null(editor.SaveCommand);
        Assert.Null(editor.CancelCommand);
        Assert.Equal("Save", editor.SaveText);
        Assert.Equal("Cancel", editor.CancelText);
        Assert.True(editor.IsSaveOnEnterEnabled);
        Assert.True(editor.IsCancelOnEscapeEnabled);
    }

    [AvaloniaFact]
    public void Actions_are_labelled()
    {
        ProfileEditorView view = new ProfileEditorView();
        Show(view);

        Assert.Equal("Cancel editing", AutomationProperties.GetName(Part<Button>(view, "PART_Cancel")));
        Assert.Equal("Save profile", AutomationProperties.GetName(Part<Button>(view, "PART_Save")));
    }

    [AvaloniaFact]
    public void Title_and_buttons_are_bound()
    {
        ProfileEditorView editor = new ProfileEditorView { Title = "Your profile", SaveText = "Apply", CancelText = "Back" };
        Show(editor);

        Assert.Equal("Your profile", Part<TextBlock>(editor, "PART_Title").Text);
        Assert.Equal("Apply", Part<Button>(editor, "PART_Save").Content);
        Assert.Equal("Back", Part<Button>(editor, "PART_Cancel").Content);
    }

    [AvaloniaFact]
    public void Fields_content_is_hosted()
    {
        TextBox field = new TextBox { Text = "Guest" };
        ProfileEditorView editor = new ProfileEditorView { Fields = field };
        Show(editor);

        Assert.Same(field, Part<ContentPresenter>(editor, "PART_Fields").Content);
    }

    [AvaloniaFact]
    public void Placeholder_is_shown_without_an_avatar()
    {
        ProfileEditorView editor = new ProfileEditorView();
        Show(editor);

        Assert.NotNull(editor.PlaceholderIcon);
        Assert.True(Part<IconPresenter>(editor, "PART_Placeholder").IsVisible);
        Assert.False(Part<Image>(editor, "PART_Image").IsVisible);
    }

    [AvaloniaFact]
    public void Avatar_replaces_the_placeholder()
    {
        ProfileEditorView editor = new ProfileEditorView { Avatar = SampleImage() };
        Show(editor);

        Assert.False(Part<IconPresenter>(editor, "PART_Placeholder").IsVisible);
        Assert.True(Part<Image>(editor, "PART_Image").IsVisible);
    }

    [AvaloniaFact]
    public void Keyboard_shortcuts_can_be_disabled()
    {
        Recorder recorder = new Recorder();
        ProfileEditorView editor = new ProfileEditorView
        {
            SaveCommand = recorder,
            CancelCommand = recorder,
            IsSaveOnEnterEnabled = false,
            IsCancelOnEscapeEnabled = false
        };
        Show(editor);

        Assert.False(Press(editor, Key.Enter).Handled);
        Assert.False(Press(editor, Key.Escape).Handled);
        Assert.Empty(recorder.Executed);
    }

    [AvaloniaFact]
    public void Save_command_runs_on_click()
    {
        Recorder recorder = new Recorder();
        ProfileEditorView editor = new ProfileEditorView { SaveCommand = recorder };
        Show(editor);

        Click(Part<Button>(editor, "PART_Save"));

        Assert.Single(recorder.Executed);
    }

    [AvaloniaFact]
    public void Enter_saves_and_escape_cancels()
    {
        Recorder save = new Recorder();
        Recorder cancel = new Recorder();
        TextBox field = new TextBox();
        ProfileEditorView editor = new ProfileEditorView
        {
            Fields = field,
            SaveCommand = save,
            CancelCommand = cancel
        };
        Show(editor);

        Assert.True(Press(field, Key.Enter).Handled);
        Assert.True(Press(field, Key.Escape).Handled);

        Assert.Single(save.Executed);
        Assert.Single(cancel.Executed);
    }

    [AvaloniaFact]
    public void Keys_without_commands_are_left_alone()
    {
        ProfileEditorView editor = new ProfileEditorView();
        Show(editor);

        Assert.False(Press(editor, Key.Enter).Handled);
        Assert.False(Press(editor, Key.Escape).Handled);
    }

    [AvaloniaFact]
    public void Cancel_command_runs_on_click()
    {
        Recorder recorder = new Recorder();
        ProfileEditorView editor = new ProfileEditorView { CancelCommand = recorder };
        Show(editor);

        Click(Part<Button>(editor, "PART_Cancel"));

        Assert.Single(recorder.Executed);
    }

    private static IImage SampleImage() => new DrawingImage(new GeometryDrawing
    {
        Brush = Brushes.Green,
        Geometry = new RectangleGeometry(new Rect(0, 0, 10, 10))
    });

    private static KeyEventArgs Press(Control control, Key key)
    {
        KeyEventArgs args = new()
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = key,
            Source = control
        };

        control.RaiseEvent(args);
        return args;
    }

    private static void Click(Button button)
    {
        Window window = (Window)TopLevel.GetTopLevel(button)!;
        window.UpdateLayout();

        Point point = button.TranslatePoint(
            new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;

        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static void Show(Control control)
    {
        Window window = new Window { Content = control, Width = 400, Height = 500 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static T Part<T>(Control control, string name) where T : Control =>
        control.GetVisualDescendants().OfType<T>().First(x => x.Name == name);

    private sealed class Recorder : ICommand
    {
        public List<object?> Executed { get; } = [];

        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => Executed.Add(parameter);
    }
}