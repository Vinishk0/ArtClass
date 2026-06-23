namespace ArtClass.Application.Dtos;

public sealed record UpdateStudentRequest(
    string FullName,
    string? Phone,
    int? Age,
    IReadOnlyList<int> GroupIds);
