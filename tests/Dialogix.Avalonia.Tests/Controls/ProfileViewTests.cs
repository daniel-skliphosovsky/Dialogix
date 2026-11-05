using System.Collections.ObjectModel;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Dialogix.Avalonia.Controls;

using ShapePath = Avalonia.Controls.Shapes.Path;

namespace Dialogix.Avalonia.Tests.Controls;

public class ProfileViewTests
{
    [AvaloniaFact]
    public void Defaults_are_expected()
    {
        ProfileView profile = new ProfileView();

        Assert.Null(profile.Avatar);
        Assert.Null(profile.PlaceholderIcon);
        Assert.Null(profile.DisplayName);
        Assert.Null(profile.Items);
        Assert.Null(profile.ItemTemplate);
        Assert.Null(profile.EditCommand);
        Assert.Null(profile.EditIcon);
        Assert.True(profile.IsEditVisible);
    }

    [AvaloniaFact]
    public void Placeholder_is_shown_without_an_avatar()
    {
        ProfileView profile = new ProfileView();
        Show(profile);

        Assert.NotNull(profile.PlaceholderIcon);
        Assert.True(Part<ShapePath>(profile, "PART_Placeholder").IsVisible);
        Assert.False(Part<Image>(profile, "PART_Image").IsVisible);
    }

    [AvaloniaFact]
    public void Avatar_replaces_the_placeholder()
    {
        ProfileView profile = new ProfileView { Avatar = SampleImage() };
        Show(profile);

        Assert.False(Part<ShapePath>(profile, "PART_Placeholder").IsVisible);
        Assert.True(Part<Image>(profile, "PART_Image").IsVisible);
    }

    [AvaloniaFact]
    public void Display_name_is_hidden_when_missing()
    {
        ProfileView profile = new ProfileView();
        Show(profile);

        Assert.False(Part<TextBlock>(profile, "PART_Name").IsVisible);
    }

    [AvaloniaFact]
    public void Display_name_is_shown_when_set()
    {
        ProfileView profile = new ProfileView { DisplayName = "Guest" };
        Show(profile);

        TextBlock name = Part<TextBlock>(profile, "PART_Name");
        Assert.True(name.IsVisible);
        Assert.Equal("Guest", name.Text);
    }

    [AvaloniaFact]
    public void Items_and_item_template_are_bound()
    {
        ObservableCollection<string> items = ["Settings", "Privacy"];
        IDataTemplate template = new FuncDataTemplate<object>((_, _) => new TextBlock());
        ProfileView profile = new ProfileView { Items = items, ItemTemplate = template };
        Show(profile);

        ItemsControl control = Part<ItemsControl>(profile, "PART_Items");
        Assert.Same(items, control.ItemsSource);
        Assert.Same(template, control.ItemTemplate);
    }

    [AvaloniaFact]
    public void Edit_button_is_visible_by_default()
    {
        ProfileView profile = new ProfileView();
        Show(profile);

        Assert.True(Part<Button>(profile, "PART_Edit").IsVisible);
    }

    [AvaloniaFact]
    public void Edit_button_can_be_hidden()
    {
        ProfileView profile = new ProfileView { IsEditVisible = false };
        Show(profile);

        Assert.False(Part<Button>(profile, "PART_Edit").IsVisible);
    }

    [AvaloniaFact]
    public void Edit_command_runs_on_click()
    {
        Recorder recorder = new Recorder();
        ProfileView profile = new ProfileView { EditCommand = recorder };
        Show(profile);

        Click(Part<Button>(profile, "PART_Edit"));

        Assert.Single(recorder.Executed);
    }

    [AvaloniaFact]
    public void Edit_icon_is_themed()
    {
        ProfileView profile = new ProfileView();
        Show(profile);

        Assert.NotNull(profile.EditIcon);
        ShapePath icon = Part<Button>(profile, "PART_Edit").GetVisualDescendants().OfType<ShapePath>().First();
        Assert.Same(profile.EditIcon, icon.Data);
    }

    private static IImage SampleImage() => new DrawingImage(new GeometryDrawing
    {
        Brush = Brushes.Red,
        Geometry = new RectangleGeometry(new Rect(0, 0, 10, 10))
    });

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
        Window window = new Window { Content = control, Width = 400, Height = 300 };
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