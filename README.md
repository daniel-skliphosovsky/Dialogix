# Dialogix

Chat controls for Avalonia. Build messenger interfaces without rebuilding the same bubbles.

Dialogix is a small set of controls: a conversation, a composer, a header, message bubbles and a
profile screen with an editor. You place them, you keep the data. There is one palette and no
dependency on the host theme, so the controls look the same in light and dark themes.

## Install

```bash
dotnet add package Dialogix.Avalonia --prerelease
```

Avalonia 12 and .NET 8 or .NET 10 are required.

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
| `ConversationListView` | Virtualized chat list with selection and an empty state. |
| `MessengerView` | Inbox, active chat and composer composed in one control. |
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

`SendCommand` gets the current text, trimmed, as its parameter, and the composer clears itself
after a successful send. `ScrollToEndIcon` accepts anything `IconPresenter` accepts.

`MessageSpacing` sets the gap between bubbles (8 by default).

### Header and composer slots

Both the header and the composer can host your own controls without a custom template. Put a
control into `LeadingContent` or `ActionContent`; `LeadingContentTemplate` and
`ActionContentTemplate` do the same for a bound view model. The header stays centered and the
composer keeps its layout.

```xml
<dx:ChatHeader Title="Guest" Status="Online">
    <dx:ChatHeader.LeadingContent>
        <Button Content="Back" />
    </dx:ChatHeader.LeadingContent>
    <dx:ChatHeader.ActionContent>
        <Button Content="More" />
    </dx:ChatHeader.ActionContent>
</dx:ChatHeader>

<dx:MessageInput Watermark="Message">
    <dx:MessageInput.LeadingContent>
        <Button Content="Attach" />
    </dx:MessageInput.LeadingContent>
    <dx:MessageInput.ActionContent>
        <Button Content="Emoji" />
    </dx:MessageInput.ActionContent>
</dx:MessageInput>
```

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

## Conversation list

`ConversationListView` is a virtualized list of chats. It takes any items, a template, and a
two-way `SelectedConversation`. The list scrolls and reuses row containers, so a long inbox stays
cheap.

```xml
<dx:ConversationListView Items="{Binding Conversations}"
                         SelectedConversation="{Binding Current, Mode=TwoWay}"
                         Height="480">
    <dx:ConversationListView.ItemTemplate>
        <DataTemplate x:DataType="vm:Conversation">
            <Grid ColumnDefinitions="Auto,*,Auto" ColumnSpacing="10">
                <Border Width="38" Height="38" CornerRadius="19"
                        Background="{DynamicResource DialogixAccentBrush}">
                    <TextBlock Text="{Binding Initials}"
                               Foreground="{DynamicResource DialogixAccentForegroundBrush}"
                               HorizontalAlignment="Center"
                               VerticalAlignment="Center" />
                </Border>
                <StackPanel Grid.Column="1" VerticalAlignment="Center">
                    <TextBlock Text="{Binding Name}" FontWeight="SemiBold" />
                    <TextBlock Text="{Binding Preview}" FontSize="12" Opacity="0.6" />
                </StackPanel>
                <TextBlock Grid.Column="2"
                           Text="{Binding Time}"
                           FontSize="11"
                           Opacity="0.5" />
            </Grid>
        </DataTemplate>
    </dx:ConversationListView.ItemTemplate>
</dx:ConversationListView>
```

Rows are selected with the pointer or the keyboard. Like `ChatView`, the control accepts
`EmptyContent` and `EmptyContentTemplate` while `IsEmpty` is true.

The list shows items in collection order. When a chat gets a notification, reorder it in the app
(for example with `ObservableCollection.Move`); the control observes `INotifyCollectionChanged`
and updates without rebuilding the list. The selected conversation stays selected after a move.

## Messenger view

`MessengerView` composes the list, the header, the conversation and the composer. It is a thin
wrapper: every part is still a normal Dialogix control, and the primitives remain available when
you need a different layout.

```xml
<dx:MessengerView Conversations="{Binding Conversations}"
                  SelectedConversation="{Binding Current, Mode=TwoWay}"
                  Messages="{Binding Current.Messages}"
                  Title="{Binding Current.Name}"
                  Status="{Binding Current.Status}"
                  SendCommand="{Binding SendCommand}">
    <dx:MessengerView.ConversationTemplate>
        <DataTemplate x:DataType="vm:Conversation">
            <TextBlock Text="{Binding Name}" />
        </DataTemplate>
    </dx:MessengerView.ConversationTemplate>

    <dx:MessengerView.MessageTemplate>
        <DataTemplate x:DataType="vm:Message">
            <dx:MessageBubble Text="{Binding Text}"
                              Author="{Binding Author}"
                              IsOutgoing="{Binding IsOutgoing}" />
        </DataTemplate>
    </dx:MessengerView.MessageTemplate>
</dx:MessengerView>
```

With no selection the right side shows `PlaceholderText`. `IsListVisible` hides the inbox and
`ListWidth` sets its width. `HasConversation` is read-only.

The chat side keeps its own empty state for a selected conversation without messages. It forwards
to the nested `ChatView`:

```xml
<dx:MessengerView SelectedConversation="{Binding Current}"
                  Messages="{Binding Current.Messages}"
                  MessagesEmptyContentTemplate="{StaticResource EmptyMessages}">
    <dx:MessengerView.MessagesEmptyContent>
        <vm:EmptyState Title="No messages yet" Hint="Say hello to start." />
    </dx:MessengerView.MessagesEmptyContent>
</dx:MessengerView>
```

Header and composer slots mirror their child controls: `HeaderLeadingContent`,
`HeaderActionContent`, `ComposerLeadingContent` and `ComposerActionContent` (plus the matching
templates).

## Message bubble

```xml
<dx:MessageBubble Text="Hello"
                  Author="Guest"
                  Timestamp="12:41"
                  IsOutgoing="False" />
```

`MaxBubbleWidth` defaults to 520, the corner radius is 12, and the text is selectable.

### Delivery status

`Status` shows a small piece of content next to the timestamp. It is hidden when null.
`StatusTemplate` renders a view model instead:

```xml
<dx:MessageBubble Text="Hello"
                  Timestamp="12:41"
                  Status="Read"
                  IsOutgoing="True" />
```

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

`ItemCommand` receives the clicked row. Rows also work from the keyboard: Tab into the list, use
the arrow keys, then Enter or Space. `SelectedItem` tracks the highlighted row; the focus ring and
selection color only appear for keyboard focus, so a pointer click does not leave a selected look.
`IsEditVisible="False"` hides the pencil. `Avatar` takes an `IImage`; `PlaceholderIcon` is shown
when there is none.

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

Enter saves and Escape cancels while the view has focus. Set `IsSaveOnEnterEnabled` or
`IsCancelOnEscapeEnabled` to `false` when the host screen already owns those keys. `BoxShadow` is a
normal property on the editor too.

## Accessibility

The built-in actions have stable automation names: `Send message`, `Scroll to latest message`,
`Edit profile`, `Save profile`, `Cancel editing` and `Profile details`. The composer's text box
takes its name from `Watermark`. The profile rows are reachable with Tab, the arrow keys and
Enter or Space.

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

All colors are brushes in one resource dictionary. Override the full palette once and every
Dialogix control follows it, in light and dark themes. No control templates are involved.

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.ThemeDictionaries>
            <ResourceDictionary x:Key="Default">
                <SolidColorBrush x:Key="DialogixSurfaceBrush" Color="#FFFFFFFF" />
                <SolidColorBrush x:Key="DialogixSurfaceForegroundBrush" Color="#FF111111" />
                <SolidColorBrush x:Key="DialogixAccentBrush" Color="#FFD6EEFF" />
                <SolidColorBrush x:Key="DialogixAccentForegroundBrush" Color="#FF111111" />
                <SolidColorBrush x:Key="DialogixBorderBrush" Color="#FFE2E2E2" />
                <SolidColorBrush x:Key="DialogixChatBackgroundBrush" Color="#FFF5F6F8" />
                <SolidColorBrush x:Key="DialogixInputBackgroundBrush" Color="#FFFFFFFF" />
                <SolidColorBrush x:Key="DialogixMutedForegroundBrush" Color="#FF5F6B7A" />
                <SolidColorBrush x:Key="DialogixPrimaryBrush" Color="#FF4F46E5" />
                <SolidColorBrush x:Key="DialogixPrimaryHoverBrush" Color="#FF5E55ED" />
                <SolidColorBrush x:Key="DialogixPrimaryPressedBrush" Color="#FF4338CA" />
                <SolidColorBrush x:Key="DialogixPrimaryForegroundBrush" Color="#FFFFFFFF" />
                <SolidColorBrush x:Key="DialogixDangerBrush" Color="#FFDC2626" />
            </ResourceDictionary>

            <ResourceDictionary x:Key="Dark">
                <SolidColorBrush x:Key="DialogixSurfaceBrush" Color="#FF2B2B2B" />
                <SolidColorBrush x:Key="DialogixSurfaceForegroundBrush" Color="#FFF2F2F2" />
                <SolidColorBrush x:Key="DialogixAccentBrush" Color="#FF1F4E6B" />
                <SolidColorBrush x:Key="DialogixAccentForegroundBrush" Color="#FFF2F2F2" />
                <SolidColorBrush x:Key="DialogixBorderBrush" Color="#FF3D3D3D" />
                <SolidColorBrush x:Key="DialogixChatBackgroundBrush" Color="#FF1C1C1C" />
                <SolidColorBrush x:Key="DialogixInputBackgroundBrush" Color="#FF2A2A2A" />
                <SolidColorBrush x:Key="DialogixMutedForegroundBrush" Color="#FF9CA3AF" />
                <SolidColorBrush x:Key="DialogixPrimaryBrush" Color="#FF4F46E5" />
                <SolidColorBrush x:Key="DialogixPrimaryHoverBrush" Color="#FF5E55ED" />
                <SolidColorBrush x:Key="DialogixPrimaryPressedBrush" Color="#FF4338CA" />
                <SolidColorBrush x:Key="DialogixPrimaryForegroundBrush" Color="#FFFFFFFF" />
                <SolidColorBrush x:Key="DialogixDangerBrush" Color="#FFF87171" />
            </ResourceDictionary>
        </ResourceDictionary.ThemeDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Put the same keys into the `Resources` of any container to recolor only that subtree:

```xml
<Border>
    <Border.Resources>
        <SolidColorBrush x:Key="DialogixSurfaceBrush" Color="#FF0F2A33" />
        <SolidColorBrush x:Key="DialogixPrimaryBrush" Color="#FF0F766E" />
    </Border.Resources>

    <dx:MessengerView />
</Border>
```

The default palette keeps text contrast at WCAG AA in both themes.

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

### One element

Every control exposes its paint as normal properties. Use them when one control should differ from
the palette; no selector and no template override is needed.

```xml
<dx:ChatHeader Title="Ocean"
               Status="Custom"
               Background="#FF0F2A33"
               Foreground="#FFE6FFFB"
               BorderBrush="#FF235A66"
               CornerRadius="14"
               BoxShadow="0 10 30 -12 #66000000" />

<dx:MessageBubble Text="Hello"
                  Background="#FF1E293B"
                  Foreground="#FFE2E8F0"
                  CornerRadius="18" />

<dx:MessageInput Watermark="Message"
                 Background="#FF0C303A"
                 Foreground="#FFE6FFFB"
                 BorderBrush="#FF235A66" />
```

`Background`, `Foreground`, `BorderBrush`, `BorderThickness`, `CornerRadius`, `Padding` and
`BoxShadow` are available where the control uses them. `ProfileEditorView` has `BoxShadow` too.
Nested parts such as the composer text box follow the parent's `Foreground` and the palette.

For your own text fields, use the library theme so they match:

```xml
<TextBox Theme="{DynamicResource DialogixTextBoxTheme}" PlaceholderText="Search" />
```

The demo also shows scoped **Matrix** skins: a green terminal palette with monospace text and square
edges, applied per card on the chat, input, profile, conversation list and messenger pages.
