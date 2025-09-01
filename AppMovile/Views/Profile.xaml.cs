using System;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Security.Cryptography;
using System.Text;

namespace AppMovile.Views
{
    public partial class Profile : ContentPage
    {
        private const string FirebaseDatabaseUrl = "https://appmobile-6ec0e-default-rtdb.firebaseio.com/";
        private readonly FirebaseClient firebase;

        public Profile()
        {
            InitializeComponent();
            firebase = new FirebaseClient(FirebaseDatabaseUrl);
            LoadUserData();
        }

        private void LoadUserData()
        {
            FullNameEntry.Text = Preferences.Get("UserFullName", string.Empty);
            EmailEntry.Text = Preferences.Get("UserEmail", string.Empty);
            PasswordEntry.Text = Preferences.Get("Password", string.Empty);
        }

        private async void OnUpdateClicked(object sender, EventArgs e)
        {
            try
            {
                // Récupération de l’ID utilisateur
                string userId = Preferences.Get("AuthUserId", string.Empty);
                if (string.IsNullOrEmpty(userId))
                {
                    await DisplayAlert("Erreur", "Utilisateur non authentifié.", "OK");
                    return;
                }

                // Charger l’ancien profil
                var existingProfile = await firebase
                    .Child("Users")
                    .Child(userId)
                    .OnceSingleAsync<UserProfile>();

                if (existingProfile == null)
                {
                    await DisplayAlert("Erreur", "Impossible de charger l’utilisateur.", "OK");
                    return;
                }

                // Mettre à jour les champs
                existingProfile.FullName = FullNameEntry.Text;

                // Email non modifiable → on ne change pas existingProfile.Email

                // Vérifier si mot de passe changé
                string newPassword = PasswordEntry.Text ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    CreatePasswordHash(newPassword, out string saltBase64, out string hashBase64);
                    existingProfile.PasswordSalt = saltBase64;
                    existingProfile.PasswordHash = hashBase64;
                }

                // Sauvegarder dans Firebase
                await firebase
                    .Child("Users")
                    .Child(userId)
                    .PutAsync(existingProfile);

                // Mise à jour locale
                Preferences.Set("UserFullName", existingProfile.FullName);
                Preferences.Set("Password", newPassword);
                Preferences.Set("UserRole", existingProfile.Role);

                await DisplayAlert("Succès", "Vos informations ont été mises à jour.", "OK");

                // 🔹 Recharge la MainPage
                Application.Current.MainPage = new NavigationPage(new MainPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Impossible de mettre à jour : {ex.Message}", "OK");
            }
        }


        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Clear();
            Application.Current.MainPage = new NavigationPage(new WelcomePage());
        }

        private void OnTogglePasswordClicked(object sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            TogglePasswordButton.Source = PasswordEntry.IsPassword ? "eye_closed.png" : "eye_open.png";
        }

        // ------------------------
        // Utilities
        // ------------------------

        private static void CreatePasswordHash(string password, out string saltBase64, out string hashBase64)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Mot de passe vide", nameof(password));

            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            using var hmac = new HMACSHA256(salt);
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            saltBase64 = Convert.ToBase64String(salt);
            hashBase64 = Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string saltBase64, string storedHashBase64)
        {
            byte[] salt = Convert.FromBase64String(saltBase64);
            byte[] expectedHash = Convert.FromBase64String(storedHashBase64);
            using var hmac = new HMACSHA256(salt);
            byte[] computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return CryptographicOperations.FixedTimeEquals(computed, expectedHash);
        }
    }

    // Garder la même structure que RegisterPage
    public class UserProfile
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string AuthUserId { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }
    }
}
