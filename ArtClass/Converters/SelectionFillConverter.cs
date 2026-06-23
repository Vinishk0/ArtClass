using System.Globalization;

namespace ArtClass.Converters;

public sealed class SelectionFillConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isSelected = value is true;
        if (!isSelected)
        {
            return Colors.Transparent;
        }

        return Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#E07A5F")
            : Color.FromArgb("#C45C3E");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
