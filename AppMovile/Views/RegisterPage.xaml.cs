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
            string fullName = FullNameEntry?.Text?.Trim();
            string email = EmailEntry?.Text?.Trim();
            string password = PasswordEntry?.Text ?? string.Empty;
            bool accepted = TermsCheckBox?.IsChecked ?? false;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Erreur", "Remplissez tous les champs.", "OK");
                return;
            }

            if (!accepted)
            {
                await DisplayAlert("Erreur", "Acceptez les conditions.", "OK");
                return;
            }

            try
            {
                // 1) Create account in Firebase Auth (this securely stores the real password).
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig(FirebaseApiKey));
                var auth = await authProvider.CreateUserWithEmailAndPasswordAsync(email, password);

                // 2) Prepare user profile data for Realtime Database
                //    We store a salted hash of the password, not the plain text.
                CreatePasswordHash(password, out string saltBase64, out string hashBase64);

                var userProfile = new UserProfile
                {
                    FullName = fullName,
                    Email = email,
                    Role = "user",
                    CreatedAtUtc = DateTime.UtcNow,
                    AuthUserId = auth.User.LocalId,
                    PasswordSalt = saltBase64,
                    PasswordHash = hashBase64,

                    // If you insist on storing the raw password (NOT RECOMMENDED), uncomment:
                    // PasswordPlain = password
                };

                // 3) Save profile to Realtime Database
                var firebaseClient = new FirebaseClient(FirebaseDatabaseUrl);
                await firebaseClient
                    .Child("Users")
                    .Child(auth.User.LocalId)
                    .PutAsync(userProfile);

                await DisplayAlert("Succès", "Compte créé avec succès!", "OK");
                // Optionally: navigate to main interface here
                // await Navigation.PushAsync(new MainPage());
            }
            catch (FirebaseAuthException fae)
            {
                await DisplayAlert("Erreur Auth", fae.Reason.ToString(), "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Creates a salted HMAC-SHA256 password hash.
        /// Store both salt and hash (Base64) to verify later.
        /// </summary>
        private static void CreatePasswordHash(string password, out string saltBase64, out string hashBase64)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be empty/whitespace.", nameof(password));

            // 16 bytes random salt
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            // HMACSHA256 with salt as key
            using var hmac = new HMACSHA256(salt);
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            saltBase64 = Convert.ToBase64String(salt);
            hashBase64 = Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Verifies a password against stored salt+hash. (Useful for custom checks later.)
        /// </summary>
        public static bool VerifyPassword(string password, string saltBase64, string storedHashBase64)
        {
            byte[] salt = Convert.FromBase64String(saltBase64);
            byte[] expectedHash = Convert.FromBase64String(storedHashBase64);
            using var hmac = new HMACSHA256(salt);
            byte[] computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return CryptographicOperations.FixedTimeEquals(computed, expectedHash);
        }

        private class UserProfile
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public string AuthUserId { get; set; }

            // Secure fields
            public string PasswordSalt { get; set; }
            public string PasswordHash { get; set; }

            public string PasswordPlain { get; set; } // NOT RECOMMENDED
        }
    }
}
