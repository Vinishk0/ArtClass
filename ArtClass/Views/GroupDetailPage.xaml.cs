using System.ComponentModel;
using ArtClass.ViewModels;

namespace ArtClass.Views;

public partial class GroupDetailPage : ContentPage
{
    private readonly GroupDetailViewModel _viewModel;
    private ToolbarItem? _deleteToolbarItem;

    public GroupDetailPage(GroupDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.LoadCommand.ExecuteAsync(null);
        UpdateDeleteToolbarItem();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_deleteToolbarItem is not null)
        {
            ToolbarItems.Remove(_deleteToolbarItem);
            _deleteToolbarItem = null;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GroupDetailViewModel.IsMasterclass))
        {
            UpdateDeleteToolbarItem();
        }
    }

    private void UpdateDeleteToolbarItem()
    {
        if (_viewModel.IsMasterclass)
        {
            if (_deleteToolbarItem is not null)
            {
                return;
            }

            _deleteToolbarItem = new ToolbarItem
            {
                Text = "Удалить",
                Command = _viewModel.DeleteCommand,
            };
            ToolbarItems.Add(_deleteToolbarItem);
            return;
        }

        if (_deleteToolbarItem is not null)
        {
            ToolbarItems.Remove(_deleteToolbarItem);
            _deleteToolbarItem = null;
        }
    }
}
