using ArtClass.ViewModels;

namespace ArtClass.Views;

public partial class SchedulePage : ContentPage
{
    public SchedulePage(ScheduleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ScheduleViewModel viewModel)
        {
            await viewModel.LoadScheduleCommand.ExecuteAsync(null);
        }
    }
}
