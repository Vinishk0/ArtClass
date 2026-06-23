namespace ArtClass.Application.Dtos;

public sealed record StudentDto(
    int Id,
    string FullName,
    string? Phone,
    int? Age,
    IReadOnlyList<int> GroupIds,
    IReadOnlyList<string> GroupNames);
