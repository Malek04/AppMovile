using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Storage;
using AppMovile.Models;

namespace AppMovile.Views;

public partial class MainPage : ContentPage
{
    private FirebaseClient firebase;
    private List<string> allCategories = new();
    private List<Course> allCourses = new(); // Tous les cours chargés une seule fois

    public List<string> Categories { get; set; } = new();

    public MainPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        // Firebase init
        firebase = new FirebaseClient("https://appmobile-6ec0e-default-rtdb.firebaseio.com/");

        // Load session data
        string fullName = Preferences.Get("UserFullName", "Utilisateur");
        string role = Preferences.Get("UserRole", "user");

        WelcomeLabel.Text = $"Salut {fullName}";

        // Show + button only for admin
        AddButton.IsVisible = role.Equals("admin", StringComparison.OrdinalIgnoreCase);

        // Load categories + courses
        LoadCategories();
        LoadCourses();
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

    private async void LoadCourses()
    {
        try
        {
            var coursesFromDb = await firebase
                .Child("courses")
                .OnceAsync<Course>();

            allCourses = coursesFromDb
                .Select(c => new Course
                {
                    Key = c.Key,
                    Title = c.Object.Title,
                    Description = c.Object.Description,
                    Category = c.Object.Category,
                    ImageUrl = c.Object.ImageUrl
                })
                .ToList();

            // Par défaut afficher tous les cours
            DisplayCourses("Toutes");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de charger les cours : {ex.Message}", "OK");
        }
    }

    private void OnCategoryClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            string selectedCategory = btn.Text;
            DisplayCourses(selectedCategory);
        }
    }

    private void DisplayCourses(string selectedCategory)
    {
        // Clear old cards
        CardsContainer.Children.Clear();

        IEnumerable<Course> items = allCourses;

        if (selectedCategory != "Toutes")
        {
            items = items.Where(i => i.Category == selectedCategory);
        }

        foreach (var course in items)
        {
            CardsContainer.Children.Add(CreateCard(course));
        }
    }

    private Frame CreateCard(Course course)
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
                new ColumnDefinition { Width = new GridLength(80) },
                new ColumnDefinition { Width = GridLength.Star }
            },
                ColumnSpacing = 10,
                Children =
            {
                new Image
                {
                    Source = string.IsNullOrEmpty(course.ImageUrl) ? "placeholder.png" : course.ImageUrl,
                    WidthRequest = 70,
                    HeightRequest = 70,
                    Aspect = Aspect.AspectFill
                },
                new Label
                {
                    Text = course.Title,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 16,
                    TextColor = Colors.Black,
                    VerticalOptions = LayoutOptions.Center
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
