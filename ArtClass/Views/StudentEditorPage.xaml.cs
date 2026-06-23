using ArtClass.ViewModels;

namespace ArtClass.Views;

public partial class StudentEditorPage : ContentPage
{
    public StudentEditorPage(StudentEditorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is StudentEditorViewModel viewModel)
        {
            _ = viewModel.InitializeCommand.ExecuteAsync(null);
        }
    }
}
