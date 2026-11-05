using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace Dialogix.Avalonia.Demo.Views.Controls;

public partial class ProfilePage : UserControl
{
    public ProfilePage()
    {
        InitializeComponent();

        ProfileItem[] items =
        [
            new ProfileItem("Settings", (Geometry)this.FindResource("SettingsIcon")!),
            new ProfileItem("Notifications", (Geometry)this.FindResource("NotificationsIcon")!),
            new ProfileItem("Privacy", (Geometry)this.FindResource("PrivacyIcon")!)
        ];

        Profile.Items = items;
        Profile.ItemTemplate = (IDataTemplate)this.FindResource("ProfileItemTemplate")!;

        VioletProfile.Items = items;
        VioletProfile.ItemTemplate = (IDataTemplate)this.FindResource("VioletRowTemplate")!;

        Profile.ItemCommand = new RelayCommand(parameter =>
        {
            ItemStatus.Text = parameter is ProfileItem item ? $"Opened {item.Text}." : null;
        });

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