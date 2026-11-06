using System.Collections.ObjectModel;
using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Dialogix.Avalonia.Controls;


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
        Assert.Null(profile.ItemCommand);
        Assert.Null(profile.SelectedItem);
    }

    [AvaloniaFact]
    public void Placeholder_is_shown_without_an_avatar()
    {
        ProfileView profile = new ProfileView();
        Show(profile);

        Assert.NotNull(profile.PlaceholderIcon);
        Assert.True(Part<IconPresenter>(profile, "PART_Placeholder").IsVisible);
        Assert.False(Part<Image>(profile, "PART_Image").IsVisible);
    }

    [AvaloniaFact]
    public void Avatar_replaces_the_placeholder()
    {
        ProfileView profile = new ProfileView { Avatar = SampleImage() };
        Show(profile);

        Assert.False(Part<IconPresenter>(profile, "PART_Placeholder").IsVisible);
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
        IconPresenter icon = Part<Button>(profile, "PART_Edit").GetVisualDescendants().OfType<IconPresenter>().First();
        Assert.Same(profile.EditIcon, icon.Source);
    }

    [AvaloniaFact]
    public void Item_command_runs_for_the_clicked_item()
    {
        Recorder recorder = new Recorder();
        ProfileView profile = new ProfileView
        {
            Items = new[] { "Settings", "Privacy" },
            ItemTemplate = RowTemplate(),
            ItemCommand = recorder
        };
        Show(profile);

        Click(RowText(profile, "Privacy"));

        Assert.Equal(["Privacy"], recorder.Executed);
    }

    [AvaloniaFact]
    public void Item_command_ignores_clicks_outside_the_items()
    {
        Recorder recorder = new Recorder();
        ProfileView profile = new ProfileView
        {
            DisplayName = "Guest",
            Items = new[] { "Settings" },
            ItemTemplate = RowTemplate(),
            ItemCommand = recorder
        };
        Show(profile);

        Click(Part<TextBlock>(profile, "PART_Name"));

        Assert.Empty(recorder.Executed);
    }

    [AvaloniaFact]
    public void Item_command_is_not_run_when_it_cannot_execute()
    {
        Recorder recorder = new Recorder { CanRun = false };
        ProfileView profile = new ProfileView
        {
            Items = new[] { "Settings" },
            ItemTemplate = RowTemplate(),
            ItemCommand = recorder
        };
        Show(profile);

        Click(RowText(profile, "Settings"));

        Assert.Empty(recorder.Executed);
    }

    [AvaloniaFact]
    public void Row_button_keeps_its_own_command()
    {
        Recorder row = new Recorder();
        Recorder item = new Recorder();
        ProfileView profile = new ProfileView
        {
            Items = new[] { "Settings" },
            ItemTemplate = new FuncDataTemplate<object>((_, _) => new Button { Content = "Settings", Command = row }),
            ItemCommand = item
        };
        Show(profile);

        Click(Part<ItemsControl>(profile, "PART_Items").GetVisualDescendants().OfType<Button>().First());

        Assert.Single(row.Executed);
        Assert.Empty(item.Executed);
    }

    [AvaloniaFact]
    public void Selected_item_follows_the_list()
    {
        ProfileView profile = new ProfileView
        {
            Items = new[] { "Settings", "Privacy" },
            ItemTemplate = RowTemplate()
        };
        Show(profile);

        ListBox list = Part<ListBox>(profile, "PART_Items");

        list.SelectedItem = "Privacy";
        Assert.Equal("Privacy", profile.SelectedItem);

        profile.SelectedItem = "Settings";
        Assert.Equal("Settings", list.SelectedItem);
    }

    [AvaloniaFact]
    public void Enter_runs_the_item_command_for_the_selected_item()
    {
        Recorder recorder = new Recorder();
        ProfileView profile = new ProfileView
        {
            Items = new[] { "Settings", "Privacy" },
            ItemTemplate = RowTemplate(),
            ItemCommand = recorder
        };
        Show(profile);

        ListBox list = Part<ListBox>(profile, "PART_Items");
        list.SelectedItem = "Privacy";

        ListBoxItem row = (ListBoxItem)list.ContainerFromIndex(1)!;
        row.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Enter,
            Source = row
        });
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(["Privacy"], recorder.Executed);
    }

    [AvaloniaFact]
    public void Space_runs_the_item_command_for_the_selected_item()
    {
        Recorder recorder = new Recorder();
        ProfileView profile = new ProfileView
        {
            Items = new[] { "Settings" },
            ItemTemplate = RowTemplate(),
            ItemCommand = recorder
        };
        Show(profile);

        ListBox list = Part<ListBox>(profile, "PART_Items");
        list.SelectedItem = "Settings";

        ListBoxItem row = (ListBoxItem)list.ContainerFromIndex(0)!;
        row.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.Space,
            Source = row
        });
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(["Settings"], recorder.Executed);
    }

    private static TextBlock RowText(ProfileView profile, string text) =>
        Part<ItemsControl>(profile, "PART_Items").GetVisualDescendants().OfType<TextBlock>().First(x => x.Text == text);

    private static IDataTemplate RowTemplate() =>
        new FuncDataTemplate<object>((text, _) => new Border
        {
            Background = Brushes.Transparent,
            Child = new TextBlock { Text = text?.ToString() }
        });

    private static void Click(Control control)
    {
        Window window = (Window)TopLevel.GetTopLevel(control)!;
        window.UpdateLayout();

        Point point = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!.Value;

        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
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

        public bool CanRun { get; init; } = true;

        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => CanRun;

        public void Execute(object? parameter) => Executed.Add(parameter);
    }
}