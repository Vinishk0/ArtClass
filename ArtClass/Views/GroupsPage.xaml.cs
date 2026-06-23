using ArtClass.ViewModels;

namespace ArtClass.Views;

public partial class GroupsPage : ContentPage
{
    public GroupsPage(GroupsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is GroupsViewModel viewModel)
        {
            _ = viewModel.EnsureLoadedCommand.ExecuteAsync(null);
        }
    }
}
