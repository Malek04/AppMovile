using AppMovile.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace AppMovile.Views;

public partial class AddCourse : ContentPage
{
    private FirebaseClient firebase;
    private string editingCourseKey = null; // used for editing an existing course

    public AddCourse()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        firebase = new FirebaseClient("https://appmobile-6ec0e-default-rtdb.firebaseio.com/");

        LoadCategories();
        LoadCourses();
    }

    // Load categories into the Picker
    private async void LoadCategories()
    {
        var categories = await firebase
            .Child("categories")
            .OnceAsync<CategoryModel>();

        CategoryPicker.ItemsSource = categories.Select(c => c.Object.Name).ToList();
    }

    // Load saved courses into the CollectionView
    private async void LoadCourses()
    {
        var courses = await firebase
            .Child("courses")
            .OnceAsync<Course>();

        CoursesCollection.ItemsSource = courses.Select(c =>
        {
            var course = c.Object;
            course.Key = c.Key; // store Firebase key for edit/delete
            return course;
        }).ToList();
    }

    // Save or update course
    private async void OnSaveCourseClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text) ||
            string.IsNullOrWhiteSpace(DescriptionEditor.Text) ||
            CategoryPicker.SelectedIndex == -1 ||
            string.IsNullOrWhiteSpace(ImageUrlEntry.Text))
        {
            await DisplayAlert("Erreur", "Veuillez remplir tous les champs.", "OK");
            return;
        }

        var course = new Course(
            TitleEntry.Text,
            DescriptionEditor.Text,
            CategoryPicker.SelectedItem.ToString(),
            ImageUrlEntry.Text
        );

        try
        {
            if (editingCourseKey == null)
            {
                // Add new course
                await firebase.Child("courses").PostAsync(course);
            }
            else
            {
                // Update existing course
                await firebase.Child("courses").Child(editingCourseKey).PutAsync(course);
                editingCourseKey = null;
            }

            // Reset form
            TitleEntry.Text = string.Empty;
            DescriptionEditor.Text = string.Empty;
            ImageUrlEntry.Text = string.Empty;
            CategoryPicker.SelectedIndex = -1;

            await DisplayAlert("Succès", "Cours enregistré avec succès.", "OK");

            // Reload courses
            LoadCourses();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Une erreur est survenue : {ex.Message}", "OK");
        }
    }

    // Edit a course
    private void OnEditCourseClicked(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        var course = button.CommandParameter as Course;

        // Fill form with existing data
        TitleEntry.Text = course.Title;
        DescriptionEditor.Text = course.Description;
        ImageUrlEntry.Text = course.ImageUrl;
        CategoryPicker.SelectedItem = course.Category;

        editingCourseKey = course.Key;
    }

    // Delete a course
    private async void OnDeleteCourseClicked(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        var course = button.CommandParameter as Course;

        bool confirm = await DisplayAlert("Supprimer", $"Voulez-vous supprimer le cours '{course.Title}' ?", "Oui", "Non");
        if (!confirm) return;

        await firebase.Child("courses").Child(course.Key).DeleteAsync();
        await DisplayAlert("Succès", "Cours supprimé.", "OK");

        LoadCourses();
    }

    private async void OnCategorieClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddCategory());
    }

    private async void OnCoursClicked(object sender, EventArgs e)
    {
        // current page
    }

    private async void OnQuestionnaireClicked(object sender, EventArgs e)
    {
        //await Navigation.PushAsync(new QuestionnairePage());
    }

    private async void OnUserButtonClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Profil", "Aller à la page utilisateur", "OK");
    }
}
