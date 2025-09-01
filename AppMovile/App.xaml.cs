using Microsoft.Maui.Storage;
using AppMovile.Views;

namespace AppMovile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Check if a session exists
        string authUserId = Preferences.Get("AuthUserId", string.Empty);
        string password = Preferences.Get("Password", string.Empty);

        if (!string.IsNullOrEmpty(authUserId) && !string.IsNullOrEmpty(password))
        {
            // Session exists → go to MainPage
            MainPage = new NavigationPage(new MainPage());
        }
        else
        {
            // No session → show WelcomePage
            MainPage = new NavigationPage(new WelcomePage());
        }
    }
}
