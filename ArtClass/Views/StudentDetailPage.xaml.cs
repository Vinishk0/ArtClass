using ArtClass.ViewModels;

namespace ArtClass.Views;

public partial class StudentDetailPage : ContentPage
{
    private readonly StudentDetailViewModel _viewModel;

    public StudentDetailPage(StudentDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadCommand.ExecuteAsync(null);
    }
}
