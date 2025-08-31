using Microsoft.Maui.Storage;

namespace AppMovile.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            // Get saved session data
            string fullName = Preferences.Get("UserFullName", "User");

            // Set UI text
            HelloLabel.Text = $"Hello {fullName}";
        }
    }
}
