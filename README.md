# Dialogix

Chat controls for Avalonia. Build messenger interfaces without rebuilding the same bubbles.

Dialogix is a small set of controls: a conversation, a composer, a header, message bubbles and a
profile screen with an editor. You place them, you keep the data. There is one palette and no
dependency on the host theme, so the controls look the same in light and dark themes.

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

Any host theme works. Dialogix draws its own `TextBox` and scrollbars from the palette, so you do
not have to fix Fluent parts in your app.

## Controls

| Control | What it does |
| --- | --- |
| `ChatHeader` | Title and status over a rounded surface. |
| `ChatView` | Scrolling conversation with auto-scroll, paging and a scroll-to-end button. |
| `MessageBubble` | One message: author, text, timestamp, incoming or outgoing. |
| `MessageInput` | Growing composer with Enter to send and Shift+Enter for a new line. |
| `ProfileView` | Avatar, display name, clickable rows and an edit button. |
| `ProfileEditorView` | Avatar, title, your fields, Save and Cancel on Enter and Escape. |
| `IconPresenter` | A `Geometry`, `IImage` or `Control` drawn with a fill and a stroke. |

## Conversation

Put the pieces where you want them. A header, a conversation and a composer are just three rows:

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

`SendCommand` gets the current text as its parameter, and the composer clears itself after a
successful send. `ScrollToEndIcon` accepts anything `IconPresenter` accepts.

## Messages as data

You can pass controls in `Messages`, or keep view models and let a template build the bubbles:

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

`Messages` only needs `INotifyCollectionChanged` to update live. Inserting at index `0` keeps the
scroll anchor, so older pages do not make the conversation jump. Long conversations are
virtualized: only the bubbles near the viewport are created.

### Empty state

`EmptyContent` fills the view until the first message arrives (when `Messages` is empty or null).
`EmptyContentTemplate` works the same way as `MessageTemplate`; `IsEmpty` is read-only and tracks
the collection.

```xml
<dx:ChatView Messages="{Binding Messages}">
    <dx:ChatView.EmptyContent>
        <StackPanel Spacing="4" HorizontalAlignment="Center">
            <TextBlock Text="No messages yet" FontWeight="SemiBold" />
            <TextBlock Text="Say hello to start." Opacity="0.6" />
        </StackPanel>
    </dx:ChatView.EmptyContent>
</dx:ChatView>
```

### Paging

Set `HasMore` while older messages exist and handle `LoadMoreCommand`. It runs when the user
reaches `LoadMoreThreshold` (48px by default) and not while `IsLoadingMore` is true. The view also
keeps itself filled: if the loaded pages are shorter than the view, it asks for another page.

```csharp
Chat.HasMore = true;
Chat.IsLoadingMore = true;
Chat.LoadMoreCommand = LoadOlderCommand;
```

While `IsLoadingMore` is true, a small pill sits at the top. The default is a flat progress bar;
you can put anything into it:

```xml
<dx:ChatView IsLoadingMore="{Binding IsLoading}">
    <dx:ChatView.LoadingContent>
        <TextBlock Text="Loading older messages…" />
    </dx:ChatView.LoadingContent>
</dx:ChatView>
```

`LoadingContent` takes a control (and `LoadingContentTemplate` takes a template if you bind a view
model). The pill itself can be restyled with a selector, like any other part:

```xml
<Style Selector="dx|ChatView /template/ Border#PART_LoadingMore">
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="BorderThickness" Value="0" />
</Style>
```

`ChatView.ScrollToEnd()` jumps to the latest message from code.

## Message bubble

```xml
<dx:MessageBubble Text="Hello"
                  Author="Guest"
                  Timestamp="12:41"
                  IsOutgoing="False" />
```

`MaxBubbleWidth` defaults to 520 and the text is selectable.

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
an `IImage`; `PlaceholderIcon` is shown when there is none.

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

Enter saves and Escape cancels while the view has focus.

## Icons

```xml
<dx:IconPresenter Width="18"
                  Height="18"
                  Source="{DynamicResource DialogixSendIconGeometry}"
                  Stroke="{DynamicResource DialogixPrimaryForegroundBrush}"
                  StrokeThickness="2" />
```

Every icon property (`SendIcon`, `ScrollToEndIcon`, `PlaceholderIcon`, `EditIcon`) takes an
`object`: a `Geometry`, an `IImage` or a ready `Control`. The built-in geometries are
`DialogixSendIconGeometry`, `DialogixScrollDownIconGeometry`, `DialogixPaperPlaneIconGeometry`,
`DialogixUserIconGeometry` and `DialogixPencilIconGeometry`.

## Theming

Everything is a resource. Override the whole app:

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

Or put the same keys into the `Resources` of any container to recolor just that subtree.

| Key | Used for |
| --- | --- |
| `DialogixSurfaceBrush` | Cards, bubbles, the header. |
| `DialogixSurfaceForegroundBrush` | Text on surfaces. |
| `DialogixChatBackgroundBrush` | The conversation background. |
| `DialogixInputBackgroundBrush` | The composer and input fields. |
| `DialogixBorderBrush` | Outlines. |
| `DialogixMutedForegroundBrush` | Statuses, placeholders, secondary text. |
| `DialogixAccentBrush` | Incoming highlights and selection. |
| `DialogixAccentForegroundBrush` | Text on the accent. |
| `DialogixPrimaryBrush` | Send and save buttons. |
| `DialogixPrimaryHoverBrush` | Primary hover. |
| `DialogixPrimaryPressedBrush` | Primary press. |
| `DialogixPrimaryForegroundBrush` | Text and icons on primary. |
| `DialogixDangerBrush` | Over-limit and error states. |

For your own text fields, use the library theme so they match:

```xml
<TextBox Theme="{DynamicResource DialogixTextBoxTheme}" PlaceholderText="Search" />
```
