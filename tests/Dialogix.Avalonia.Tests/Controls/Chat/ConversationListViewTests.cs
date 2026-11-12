using System.Collections.ObjectModel;

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;

using Dialogix.Avalonia.Controls;

namespace Dialogix.Avalonia.Tests.Controls.Chat;

public class ConversationListViewTests
{
    [AvaloniaFact]
    public void Defaults_are_expected()
    {
        ConversationListView list = new ConversationListView();

        Assert.Null(list.Items);
        Assert.Null(list.ItemTemplate);
        Assert.Null(list.SelectedConversation);
        Assert.Null(list.EmptyContent);
        Assert.Null(list.EmptyContentTemplate);
        Assert.True(list.IsEmpty);
    }

    [AvaloniaFact]
    public void Items_and_template_are_bound()
    {
        ObservableCollection<string> items = ["Settings", "Privacy"];
        IDataTemplate template = new FuncDataTemplate<object>((item, _) => new TextBlock { Text = item?.ToString() });
        ConversationListView list = new ConversationListView { Items = items, ItemTemplate = template };
        Show(list);

        ListBox inner = Part<ListBox>(list, "PART_Items");
        Assert.Same(items, inner.ItemsSource);
        Assert.Same(template, inner.ItemTemplate);
        Assert.False(list.IsEmpty);
    }

    [AvaloniaFact]
    public void Selection_flows_both_ways()
    {
        ConversationListView list = new ConversationListView
        {
            Items = new[] { "Settings", "Privacy" },
            ItemTemplate = new FuncDataTemplate<object>((item, _) => new TextBlock { Text = item?.ToString() }),
            SelectedConversation = "Privacy"
        };
        Show(list);

        ListBox inner = Part<ListBox>(list, "PART_Items");
        Assert.Equal("Privacy", inner.SelectedItem);

        inner.SelectedItem = "Settings";
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Settings", list.SelectedConversation);
    }

    [AvaloniaFact]
    public void Moving_the_selected_item_keeps_selection()
    {
        ObservableCollection<string> items = ["Settings", "Privacy", "About"];
        ConversationListView list = new ConversationListView
        {
            Items = items,
            ItemTemplate = new FuncDataTemplate<object>((item, _) => new TextBlock { Text = item?.ToString() }),
            SelectedConversation = "Privacy"
        };
        Show(list);

        ListBox inner = Part<ListBox>(list, "PART_Items");

        items.Move(1, 0);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Privacy", list.SelectedConversation);
        Assert.Equal("Privacy", inner.SelectedItem);
    }

    [AvaloniaFact]
    public void Empty_state_tracks_the_collection()
    {
        ObservableCollection<string> items = [];
        TextBlock empty = new() { Text = "No conversations yet" };
        ConversationListView list = new ConversationListView
        {
            Items = items,
            ItemTemplate = new FuncDataTemplate<object>((item, _) => new TextBlock { Text = item?.ToString() }),
            EmptyContent = empty
        };
        Show(list);

        ContentPresenter presenter = Part<ContentPresenter>(list, "PART_Empty");
        Assert.True(presenter.IsVisible);
        Assert.Same(empty, presenter.Content);

        items.Add("Settings");
        Dispatcher.UIThread.RunJobs();
        Assert.False(presenter.IsVisible);

        items.Remove("Settings");
        Dispatcher.UIThread.RunJobs();
        Assert.True(presenter.IsVisible);
    }

    [AvaloniaFact]
    public void List_is_virtualized_and_uses_the_library_scrollbar()
    {
        ConversationListView list = new ConversationListView
        {
            Items = Enumerable.Range(0, 100).Select(x => x.ToString()).ToArray(),
            ItemTemplate = new FuncDataTemplate<object>((item, _) => new TextBlock { Text = item?.ToString() })
        };
        Show(list);

        ListBox inner = Part<ListBox>(list, "PART_Items");
        Assert.IsType<VirtualizingStackPanel>(inner.ItemsPanelRoot);

        ScrollViewer scroll = inner.GetVisualDescendants().OfType<ScrollViewer>().First();
        Assert.NotNull(scroll.Theme);
    }

    [AvaloniaFact]
    public void List_is_labelled()
    {
        ConversationListView list = new ConversationListView();
        Show(list);

        Assert.Equal("Conversations", AutomationProperties.GetName(Part<ListBox>(list, "PART_Items")));
    }

    [AvaloniaFact]
    public void Detaching_stops_tracking_the_collection()
    {
        ObservableCollection<string> items = [];
        ConversationListView list = new ConversationListView { Items = items };
        Window window = new Window { Content = list, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Assert.True(list.IsEmpty);

        window.Content = null;
        Dispatcher.UIThread.RunJobs();

        items.Add("Settings");
        Dispatcher.UIThread.RunJobs();

        Assert.True(list.IsEmpty);

        window.Content = list;
        Dispatcher.UIThread.RunJobs();

        Assert.False(list.IsEmpty);
    }

    private static void Show(Control control)
    {
        Window window = new Window { Content = control, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static T Part<T>(Control control, string name) where T : Control =>
        control.GetVisualDescendants().OfType<T>().First(x => x.Name == name);
}