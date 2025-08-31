using Microsoft.Maui.Controls;

namespace AppMovile.Views
{
    public partial class Profile : ContentPage
    {
        private bool isPasswordVisible = false;

        public Profile()
        {
            InitializeComponent();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void OnTogglePasswordVisibility(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            PasswordEntry.IsPassword = !isPasswordVisible;
        }

        private async void OnUpdateClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Info", "Mise à jour effectuée !", "OK");
            // TODO: save updates to Firebase
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            // Clear session / token here if needed
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}
