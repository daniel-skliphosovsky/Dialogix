using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace Dialogix.Avalonia.Demo.Views.Controls;

public partial class ProfilePage : UserControl
{
    public ProfilePage()
    {
        InitializeComponent();

        Profile.Items = new[]
        {
            new ProfileItem("Settings", (Geometry)this.FindResource("SettingsIcon")!),
            new ProfileItem("Notifications", (Geometry)this.FindResource("NotificationsIcon")!),
            new ProfileItem("Privacy", (Geometry)this.FindResource("PrivacyIcon")!)
        };
        Profile.ItemTemplate = (IDataTemplate)this.FindResource("ProfileItemTemplate")!;

        Profile.EditCommand = new RelayCommand(_ => ShowEditor(true));

        Editor.SaveCommand = new RelayCommand(_ =>
        {
            Profile.DisplayName = NameBox.Text;
            ShowEditor(false);
        });

        Editor.CancelCommand = new RelayCommand(_ =>
        {
            NameBox.Text = Profile.DisplayName;
            ShowEditor(false);
        });
    }

    private void ShowEditor(bool editor)
    {
        Profile.IsVisible = !editor;
        Editor.IsVisible = editor;
    }
}

public sealed record ProfileItem(string Text, Geometry Icon);