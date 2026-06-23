using ArtClass.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace ArtClass.Views;

public partial class CalendarPage : ContentPage
{
    private const int Columns = 7;
    private const int Rows = 6;

    private readonly CalendarViewModel _viewModel;
    private readonly CalendarCell[] _cells = new CalendarCell[42];

    public CalendarPage(CalendarViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        CalendarGrid.SizeChanged += (_, _) => UpdateSquareCellSize();
        BuildCalendarGrid();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.EnsureLoadedCommand.ExecuteAsync(null);
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CalendarViewModel.Days))
        {
            RenderDays(_viewModel.Days);
        }
    }

    private void UpdateSquareCellSize()
    {
        if (CalendarGrid.Width <= 0)
        {
            return;
        }

        var spacing = CalendarGrid.ColumnSpacing;
        var cellSize = (CalendarGrid.Width - spacing * (Columns - 1)) / Columns;
        CalendarGrid.HeightRequest = cellSize * Rows + spacing * (Rows - 1);
    }

    private void BuildCalendarGrid()
    {
        for (var index = 0; index < _cells.Length; index++)
        {
            var cell = new CalendarCell(OnCellTapped);
            _cells[index] = cell;
            CalendarGrid.Add(cell.Root, index % Columns, index / Columns);
        }

        RenderDays(_viewModel.Days);
    }

    private void RenderDays(IReadOnlyList<CalendarDayItem> days)
    {
        for (var index = 0; index < _cells.Length; index++)
        {
            var day = index < days.Count ? days[index] : CalendarDayItem.Empty;
            _cells[index].Bind(day);
        }
    }

    private async void OnCellTapped(CalendarDayItem day)
    {
        await _viewModel.OpenDayCommand.ExecuteAsync(day);
    }

    private sealed class CalendarCell
    {
        private static readonly Color TodayStrokeLight = Color.FromArgb("#D4A853");
        private static readonly Color TodayStrokeDark = Color.FromArgb("#FBBF24");

        private readonly Action<CalendarDayItem> _onTap;
        private readonly Grid _colorStrip;
        private CalendarDayItem _day = CalendarDayItem.Empty;

        public CalendarCell(Action<CalendarDayItem> onTap)
        {
            _onTap = onTap;

            DayLabel = new Label
            {
                FontFamily = "OpenSansSemibold",
                FontSize = 13,
                Margin = new Thickness(0, 2, 4, 0),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
            };

            _colorStrip = new Grid
            {
                ColumnSpacing = 0,
                HeightRequest = 5,
                VerticalOptions = LayoutOptions.End,
                IsVisible = false,
            };

            var layout = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(5),
                },
                Padding = new Thickness(2),
            };
            layout.Add(DayLabel, 0, 0);
            layout.Add(_colorStrip, 0, 1);

            Root = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 4 },
                Padding = 0,
                Content = layout,
            };

            DayLabel.SetAppThemeColor(
                Label.TextColorProperty,
                (Color)Microsoft.Maui.Controls.Application.Current!.Resources["TextPrimary"],
                (Color)Microsoft.Maui.Controls.Application.Current.Resources["TextPrimaryDark"]);

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) =>
            {
                if (_day.DayNumber > 0)
                {
                    _onTap(_day);
                }
            };
            Root.GestureRecognizers.Add(tap);
        }

        public Border Root { get; }
        public Label DayLabel { get; }

        public void Bind(CalendarDayItem day)
        {
            if (_day.HasSamePresentationAs(day))
            {
                _day = day;
                return;
            }

            _day = day;
            DayLabel.Text = day.DayNumber > 0 ? day.DayNumber.ToString() : string.Empty;
            Root.Opacity = day.CellOpacity;

            Root.SetAppThemeColor(Border.BackgroundColorProperty, day.FillLight, day.FillDark);
            UpdateTodayStroke(day);
            UpdateColorStrip(day);
        }

        private void UpdateTodayStroke(CalendarDayItem day)
        {
            if (day.IsToday)
            {
                Root.StrokeThickness = 2;
                Root.SetAppThemeColor(Border.StrokeProperty, TodayStrokeLight, TodayStrokeDark);
                return;
            }

            Root.StrokeThickness = 0;
            Root.Stroke = Colors.Transparent;
        }

        private void UpdateColorStrip(CalendarDayItem day)
        {
            _colorStrip.Children.Clear();
            _colorStrip.ColumnDefinitions.Clear();

            if (day.AccentColors.Count <= 1)
            {
                _colorStrip.IsVisible = false;
                return;
            }

            _colorStrip.IsVisible = true;
            for (var i = 0; i < day.AccentColors.Count; i++)
            {
                _colorStrip.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                _colorStrip.Add(new BoxView { Color = day.AccentColors[i] }, i, 0);
            }
        }
    }
}
