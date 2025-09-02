using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Storage;
using AppMovile.Models;

namespace AppMovile.Views;

public partial class MainPage : ContentPage
{
    private FirebaseClient firebase;
    private List<string> allCategories = new();

    public List<string> Categories { get; set; } = new();

    public MainPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        // Firebase init (replace with your DB URL)
        firebase = new FirebaseClient("https://appmobile-6ec0e-default-rtdb.firebaseio.com/");

        // Load session data
        string fullName = Preferences.Get("UserFullName", "Utilisateur");
        string role = Preferences.Get("UserRole", "user");

        WelcomeLabel.Text = $"Salut {fullName}";

        // Show + button only for admin
        AddButton.IsVisible = role.Equals("admin", StringComparison.OrdinalIgnoreCase);

        // Load categories
        LoadCategories();
    }

    private async void LoadCategories()
    {
        try
        {
            var categoriesFromDb = await firebase
                .Child("categories") 
                .OnceAsync<CategoryModel>();

            allCategories = categoriesFromDb
                .Select(c => c.Object?.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            Categories.Clear();
            Categories.Add("Toutes");
            Categories.AddRange(allCategories);

            BindableLayout.SetItemsSource(CategoryStackLayout, null);
            BindableLayout.SetItemsSource(CategoryStackLayout, Categories);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de charger les catégories : {ex.Message}", "OK");
        }
    }






    private void OnCategoryClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            string selectedCategory = btn.Text;

            // Clear old cards
            CardsContainer.Children.Clear();

            // Filter logic
            var items = GetSampleItems(); // Replace with Firebase items query later

            if (selectedCategory != "Toutes")
            {
                items = items.Where(i => i.Category == selectedCategory).ToList();
            }

            // Render filtered items
            foreach (var item in items)
            {
                CardsContainer.Children.Add(CreateCard(item.Title, item.Description));
            }
        }
    }

    // Example item class
    private class Item
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }

    // Fake data (replace with Firebase query for items)
    private List<Item> GetSampleItems()
    {
        return new List<Item>
        {
            new Item { Title = "Titre 1", Description = "Description 1", Category = "Categorie 1" },
            new Item { Title = "Titre 2", Description = "Description 2", Category = "Categorie 2" },
            new Item { Title = "Titre 3", Description = "Description 3", Category = "Categorie 3" },
        };
    }

    private Frame CreateCard(string title, string description)
    {
        return new Frame
        {
            CornerRadius = 10,
            Padding = 10,
            BackgroundColor = Color.FromArgb("#F8F5F2"),
            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(60) },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                ColumnSpacing = 10,
                Children =
                {
                    new Image { Source = "placeholder.png", WidthRequest = 50, HeightRequest = 50 },
                    new VerticalStackLayout
                    {
                        Children =
                        {
                            new Label { Text = title, FontAttributes = FontAttributes.Bold, FontSize = 16 },
                            new Label { Text = description, FontSize = 14, TextColor = Colors.Gray, LineBreakMode = LineBreakMode.WordWrap }
                        }
                    }.Assign(Grid.ColumnProperty, 1)
                }
            }
        };
    }

    private async void OnUserButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Profile());
    }

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddCategory());
    }
}

// Helper extension
public static class ViewExtensions
{
    public static T Assign<T>(this T self, BindableProperty property, object value) where T : BindableObject
    {
        self.SetValue(property, value);
        return self;
    }
}
