using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage; // for Preferences

namespace AppMovile.Views
{
    public partial class LoginPage : ContentPage
    {
        private const string FirebaseApiKey = "AIzaSyDYJK9V5mO8mhMHtfPaCbI2Z-YaBItKlqA";
        private const string FirebaseDatabaseUrl = "https://appmobile-6ec0e-default-rtdb.firebaseio.com/";

        public LoginPage()
        {
            InitializeComponent();
        }

        private void OnRegisterClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new RegisterPage());
        }

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text?.Trim();
            string password = PasswordEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Erreur", "Remplissez tous les champs.", "OK");
                return;
            }

            try
            {
                // 1. Authentification Firebase
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig(FirebaseApiKey));
                var auth = await authProvider.SignInWithEmailAndPasswordAsync(email, password);

                if (auth.User == null || string.IsNullOrEmpty(auth.User.LocalId))
                {
                    await DisplayAlert("Erreur", "Utilisateur invalide. LocalId introuvable.", "OK");
                    return;
                }

                // 2. Lecture des infos utilisateur dans la DB
                var firebaseClient = new FirebaseClient(FirebaseDatabaseUrl);
                var userData = await firebaseClient
                    .Child("Users")
                    .Child(auth.User.LocalId)
                    .OnceSingleAsync<UserModel>();

                if (userData == null)
                {
                    await DisplayAlert("Erreur", "Utilisateur introuvable dans la base de données.", "OK");
                    return;
                }

                // 3. Sauvegarde des données utilisateur (Session)

                Preferences.Set("UserFullName", userData.FullName ?? "");
                Preferences.Set("UserEmail", userData.Email ?? "");
                Preferences.Set("UserRole", userData.Role ?? "");

                // 4. Navigation vers MainPage
                Application.Current.MainPage = new NavigationPage(new MainPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Crash détecté : {ex.Message}", "OK");
            }
        }
    }

    public class UserModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
