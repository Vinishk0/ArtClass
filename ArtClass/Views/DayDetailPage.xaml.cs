using ArtClass.ViewModels;

namespace ArtClass.Views;

public partial class DayDetailPage : ContentPage
{
    private readonly DayDetailViewModel _viewModel;

    public DayDetailPage(DayDetailViewModel viewModel)
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
