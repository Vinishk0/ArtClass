using ArtClass.ViewModels;

namespace ArtClass.Views;

public partial class GroupEditorPage : ContentPage
{
    public GroupEditorPage(GroupEditorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is GroupEditorViewModel viewModel)
        {
            _ = viewModel.InitializeCommand.ExecuteAsync(null);
        }
    }
}
