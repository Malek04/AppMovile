using System;
using Firebase.Auth;
using AppMovile.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace AppMovile.Views;

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
            // Disable button and show loader
            LoginButton.IsEnabled = false;
            LoginButton.Text = string.Empty;
            LoginLoader.IsVisible = true;
            LoginLoader.IsRunning = true;

            // Firebase authentication
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(FirebaseApiKey));
            var auth = await authProvider.SignInWithEmailAndPasswordAsync(email, password);

            if (auth.User == null || string.IsNullOrEmpty(auth.User.LocalId))
            {
                await DisplayAlert("Erreur", "Utilisateur invalide. LocalId introuvable.", "OK");
                return;
            }

            // Get user data from Firebase DB
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

            // Save session
            Preferences.Set("AuthUserId", auth.User.LocalId);
            Preferences.Set("UserFullName", userData.FullName ?? "");
            Preferences.Set("UserEmail", userData.Email ?? "");
            Preferences.Set("UserRole", userData.Role ?? "");
            Preferences.Set("Password", password);

            // Navigate to MainPage
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Crash détecté : {ex.Message}", "OK");
        }
        finally
        {
            if (Application.Current.MainPage is not NavigationPage)
            {
                LoginButton.IsEnabled = true;
                LoginButton.Text = "Se connecter";
                LoginLoader.IsVisible = false;
                LoginLoader.IsRunning = false;
            }
        }
    }

    private void TogglePasswordButton_Clicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        TogglePasswordButton.Source = PasswordEntry.IsPassword ? "eye_closed.png" : "eye_open.png";
    }

    // 🔹 Forgot password
    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Erreur", "Veuillez entrer votre adresse email pour réinitialiser le mot de passe.", "OK");
            return;
        }

        try
        {
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(FirebaseApiKey));
            await authProvider.SendPasswordResetEmailAsync(email);

            await DisplayAlert("Succès", $"Un email de réinitialisation a été envoyé à {email}.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible d'envoyer l'email : {ex.Message}", "OK");
        }
    }
}
