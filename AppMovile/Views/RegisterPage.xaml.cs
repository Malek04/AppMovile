using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;

namespace AppMovile.Views
{
    public partial class RegisterPage : ContentPage
    {
        private const string FirebaseApiKey = "AIzaSyDYJK9V5mO8mhMHtfPaCbI2Z-YaBItKlqA";
        private const string FirebaseDatabaseUrl = "https://appmobile-6ec0e-default-rtdb.firebaseio.com/";

        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Clicked(object sender, EventArgs e)
        {
            RegisterButton.IsEnabled = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            string fullName = FullNameEntry?.Text?.Trim();
            string email = EmailEntry?.Text?.Trim();
            string password = PasswordEntry?.Text ?? string.Empty;
            bool accepted = TermsCheckBox?.IsChecked ?? false;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Erreur", "Remplissez tous les champs.", "OK");
                ResetUI();
                return;
            }

            if (!accepted)
            {
                await DisplayAlert("Erreur", "Acceptez les conditions.", "OK");
                ResetUI();
                return;
            }

            try
            {
                // Authentification Firebase
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig(FirebaseApiKey));
                var auth = await authProvider.CreateUserWithEmailAndPasswordAsync(email, password);

                // Hash & salt du mot de passe
                CreatePasswordHash(password, out string saltBase64, out string hashBase64);

                var userProfile = new UserProfile
                {
                    FullName = fullName,
                    Email = email,
                    Role = "user",
                    CreatedAtUtc = DateTime.UtcNow,
                    AuthUserId = auth.User.LocalId,
                    PasswordSalt = saltBase64,
                    PasswordHash = hashBase64
                };

                // Sauvegarde dans Firebase Realtime DB
                var firebaseClient = new FirebaseClient(FirebaseDatabaseUrl);
                await firebaseClient
                    .Child("Users")
                    .Child(auth.User.LocalId)
                    .PutAsync(userProfile);

                await DisplayAlert("Succès", "Compte créé avec succès!", "OK");

                // Redirection vers LoginPage
                await Navigation.PushAsync(new LoginPage());
            }
            catch (FirebaseAuthException fae)
            {
                await DisplayAlert("Erreur Auth", fae.Reason.ToString(), "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", ex.Message, "OK");
            }
            finally
            {
                ResetUI();
            }
        }

        private void ResetUI()
        {
            RegisterButton.IsEnabled = true;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }

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
        private void TogglePasswordButton_Clicked(object sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            TogglePasswordButton.Source = PasswordEntry.IsPassword ? "eye_closed.png" : "eye_open.png";
        }


        private class UserProfile
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
}
