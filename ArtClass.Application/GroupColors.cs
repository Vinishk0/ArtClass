namespace ArtClass.Application;

public static class GroupColors
{
    public const string Default = "#C45C3E";

    public static readonly string[] Palette =
    [
        "#C45C3E",
        "#5B7B6A",
        "#D4A853",
        "#8B5A6B",
        "#4A6FA5",
        "#7B6B8A",
        "#B8860B",
        "#6B8E7B",
        "#A0522D",
    ];

    public static string Normalize(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return Default;
        }

        var value = color.Trim();
        return value.StartsWith('#') ? value : $"#{value}";
    }

    public static string PickDefault(int index) =>
        Palette[Math.Abs(index) % Palette.Length];
}
