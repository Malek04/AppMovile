using AppMovile.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace AppMovile.Views;

public partial class AddCategory : ContentPage
{
    private readonly FirebaseClient firebase;

    public AddCategory()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        // Connect Firebase
        firebase = new FirebaseClient("https://appmobile-6ec0e-default-rtdb.firebaseio.com/");
        LoadCategories();
    }
    private async void OnCategorieClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddCategory());
    }

    // Navigate to AddCourse page (current page)
    private async void OnCoursClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddCourse());
    }

    // Navigate to Questionnaire page
    private async void OnQuestionnaireClicked(object sender, EventArgs e)
    {
        //await Navigation.PushAsync(new QuestionnairePage());
    }
    private async void LoadCategories()
    {
        var categories = await firebase
            .Child("categories")
            .OnceAsync<CategoryModel>();

        CategoriesList.ItemsSource = categories.Select(c => new CategoryModel
        {
            Id = c.Object.Id,
            Name = c.Object.Name
        }).ToList();
    }

    private async void OnNewCategoryClicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Nouvelle Catégorie", "Nom de la catégorie:");

        if (!string.IsNullOrWhiteSpace(result))
        {
            var newCategory = new CategoryModel { Name = result };
            await firebase.Child("categories").PostAsync(newCategory);
            LoadCategories();
        }
    }

    private async void OnEditCategoryClicked(object sender, EventArgs e)
    {
        var button = (ImageButton)sender;
        var category = (CategoryModel)button.CommandParameter;

        string result = await DisplayPromptAsync("Modifier Catégorie", "Nouveau nom:", initialValue: category.Name);

        if (!string.IsNullOrWhiteSpace(result))
        {
            category.Name = result;

            // Update Firebase
            var toUpdate = (await firebase
                .Child("categories")
                .OnceAsync<CategoryModel>())
                .FirstOrDefault(c => c.Object.Id == category.Id);

            if (toUpdate != null)
            {
                await firebase
                    .Child("categories")
                    .Child(toUpdate.Key)
                    .PutAsync(category);
            }

            LoadCategories();
        }
    }

    private async void OnDeleteCategoryClicked(object sender, EventArgs e)
    {
        var button = (ImageButton)sender;
        var category = (CategoryModel)button.CommandParameter;

        bool confirm = await DisplayAlert("Supprimer", $"Supprimer {category.Name} ?", "Oui", "Non");

        if (confirm)
        {
            var toDelete = (await firebase
                .Child("categories")
                .OnceAsync<CategoryModel>())
                .FirstOrDefault(c => c.Object.Id == category.Id);

            if (toDelete != null)
            {
                await firebase
                    .Child("categories")
                    .Child(toDelete.Key)
                    .DeleteAsync();
            }

            LoadCategories();
        }
    }

    private async void OnUserButtonClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Profil", "Aller à la page utilisateur", "OK");
    }
}
