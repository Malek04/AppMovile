using Microsoft.Maui.Storage;

namespace AppMovile.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        // Get saved session data
        string fullName = Preferences.Get("UserFullName", "Utilisateur");
        string role = Preferences.Get("UserRole", "user");

        // Set welcome text
        WelcomeLabel.Text = $"Salut {fullName}";

        // Show the + button only for admin
        if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            AddButton.IsVisible = true;
        }
        else
        {
            AddButton.IsVisible = false;
        }
        }

        private async void OnUserButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Profile());
        }

}

