namespace ArtClass.Application.Dtos;

public sealed record CreateStudentRequest(
    string FullName,
    string? Phone,
    int? Age,
    IReadOnlyList<int> GroupIds);
