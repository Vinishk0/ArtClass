using ArtClass.ViewModels;

namespace ArtClass.Views;

public partial class StudentsPage : ContentPage
{
    public StudentsPage(StudentsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is StudentsViewModel viewModel)
        {
            _ = viewModel.EnsureLoadedCommand.ExecuteAsync(null);
        }
    }
}
