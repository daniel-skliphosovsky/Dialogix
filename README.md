# Dialogix

Chat controls for Avalonia. Build messenger interfaces without rebuilding the same bubbles.

Dialogix gives you a small set of `TemplatedControl`s, one palette and no hard dependency on the
host theme: bubbles, a scrollable conversation, a composer, a header and a profile screen with an
editor. You place them, you own the data.

## Install

```bash
dotnet add package Dialogix.Avalonia --prerelease
```

Avalonia 12 and .NET 10 are required.

## Include the styles

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.App">
    <Application.Styles>
        <FluentTheme />
        <StyleInclude Source="avares://Dialogix.Avalonia/Themes/Generic.axaml" />
    </Application.Styles>
</Application>
```

Any host theme works. Dialogix paints its own `TextBox` and scrollbars from its palette, so the
controls look the same everywhere.

## Controls

| Control | What it does |
| --- | --- |
| `ChatHeader` | Title and status over a rounded surface. |
| `ChatView` | Scrolling conversation with auto-scroll, pagination and a scroll-to-end button. |
| `MessageBubble` | One message: author, text, timestamp, incoming/outgoing. |
| `MessageInput` | Growing composer with Enter to send, Shift+Enter for a new line and an optional counter. |
| `ProfileView` | Avatar, display name, clickable rows and an edit button. |
| `ProfileEditorView` | Avatar, title, arbitrary fields, Save/Cancel with Enter/Escape. |
| `IconPresenter` | A `Geometry`, `IImage` or `Control` drawn with a fill/stroke. |

## Conversation

```xml
<Grid RowDefinitions="Auto,*,Auto" Height="520">
    <dx:ChatHeader Grid.Row="0" Title="Guest" Status="Online" />

    <dx:ChatView Grid.Row="1" x:Name="Chat" />

    <dx:MessageInput Grid.Row="2"
                     x:Name="Input"
                     Watermark="Message"
                     MaxLength="500"
                     MaxLines="5" />
</Grid>
```

```csharp
public MainWindow()
{
    InitializeComponent();

    ObservableCollection<MessageBubble> messages = [];

    Chat.Messages = messages;
    Chat.HasMore = true;
    Chat.LoadMoreCommand = new RelayCommand(_ => LoadOlder(messages));

    Input.SendCommand = new RelayCommand(parameter =>
    {
        if (parameter is not string text || string.IsNullOrWhiteSpace(text))
            return;

        messages.Add(new MessageBubble { Text = text, Author = "You", IsOutgoing = true });
        Chat.ScrollToEnd();
    });
}
```

`SendCommand` receives the current text as its parameter. The composer clears itself after a
successful send. `ScrollToEndIcon` accepts anything `IconPresenter` accepts.

## Messages as data

Pass controls in `Messages`, or keep view models and let a `DataTemplate` build the bubbles:

```xml
<dx:ChatView Messages="{Binding Messages}">
    <dx:ChatView.MessageTemplate>
        <DataTemplate x:DataType="vm:Message">
            <dx:MessageBubble Text="{Binding Text}"
                              Author="{Binding Author}"
                              Timestamp="{Binding Time}"
                              IsOutgoing="{Binding IsOutgoing}" />
        </DataTemplate>
    </dx:ChatView.MessageTemplate>
</dx:ChatView>
```

```csharp
public ObservableCollection<Message> Messages { get; } = [];
```

`Messages` only needs `INotifyCollectionChanged` to live-update. Inserting at index `0` keeps the
scroll anchor, so loading older pages does not jump.

### Paging

Set `HasMore` while older messages exist and handle `LoadMoreCommand`. It runs when the user
reaches `LoadMoreThreshold` (48px by default) and not while `IsLoadingMore` is true. The view
also keeps the viewport filled: if the loaded pages are shorter than the view, it asks for
another page.

```csharp
Chat.HasMore = true;
Chat.IsLoadingMore = true;      // shows the loading pill
Chat.LoadMoreCommand = LoadOlderCommand;
```

`ChatView.ScrollToEnd()` jumps to the latest message programmatically.

## Message bubble

```xml
<dx:MessageBubble Text="Hello"
                  Author="Guest"
                  Timestamp="12:41"
                  IsOutgoing="False" />
```

`MaxBubbleWidth` defaults to 520. `Text` is selectable.

## Profile

```xml
<dx:ProfileView DisplayName="Guest"
                Items="{Binding Rows}"
                ItemCommand="{Binding OpenRowCommand}"
                EditCommand="{Binding EditCommand}">
    <dx:ProfileView.ItemTemplate>
        <DataTemplate x:DataType="vm:Row">
            <Border Padding="12,10">
                <TextBlock Text="{Binding Title}" />
            </Border>
        </DataTemplate>
    </dx:ProfileView.ItemTemplate>
</dx:ProfileView>
```

`ItemCommand` receives the clicked row. `IsEditVisible="False"` hides the pencil. `Avatar` takes
a single `IImage`; `PlaceholderIcon` is used when there is none.

```xml
<dx:ProfileEditorView Title="Edit profile"
                      SaveCommand="{Binding SaveCommand}"
                      CancelCommand="{Binding CancelCommand}">
    <dx:ProfileEditorView.Fields>
        <StackPanel Spacing="8">
            <TextBox Theme="{DynamicResource DialogixTextBoxTheme}" Text="{Binding Name}" />
        </StackPanel>
    </dx:ProfileEditorView.Fields>
</dx:ProfileEditorView>
```

Enter saves, Escape cancels; both only fire while the view has focus.

## Icons

```xml
<dx:IconPresenter Width="18"
                  Height="18"
                  Source="{DynamicResource DialogixSendIconGeometry}"
                  Stroke="{DynamicResource DialogixPrimaryForegroundBrush}"
                  StrokeThickness="2" />
```

Every icon property (`SendIcon`, `ScrollToEndIcon`, `PlaceholderIcon`, `EditIcon`) takes an
`object`: a `Geometry`, an `IImage`, or a ready `Control`. Built-in geometries
(`DialogixSendIconGeometry`, `DialogixScrollDownIconGeometry`, `DialogixPaperPlaneIconGeometry`,
`DialogixUserIconGeometry`, `DialogixPencilIconGeometry`) live in the theme resources.

## Theming

Every brush is a resource. Override the whole app:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.ThemeDictionaries>
            <ResourceDictionary x:Key="Default">
                <SolidColorBrush x:Key="DialogixPrimaryBrush" Color="#FF6366F1" />
            </ResourceDictionary>
            <ResourceDictionary x:Key="Dark">
                <SolidColorBrush x:Key="DialogixPrimaryBrush" Color="#FF8B5CF6" />
            </ResourceDictionary>
        </ResourceDictionary.ThemeDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Or put the same keys into the `Resources` of any container to recolor only that subtree.

Keys:

`DialogixSurfaceBrush`, `DialogixSurfaceForegroundBrush`, `DialogixChatBackgroundBrush`,
`DialogixInputBackgroundBrush`, `DialogixBorderBrush`, `DialogixMutedForegroundBrush`,
`DialogixAccentBrush`, `DialogixAccentForegroundBrush`, `DialogixPrimaryBrush`,
`DialogixPrimaryHoverBrush`, `DialogixPrimaryPressedBrush`, `DialogixPrimaryForegroundBrush`,
`DialogixDangerBrush`.

For your own text fields use the library theme so they match:

```xml
<TextBox Theme="{DynamicResource DialogixTextBoxTheme}" PlaceholderText="Search" />
```

## License

MIT.
