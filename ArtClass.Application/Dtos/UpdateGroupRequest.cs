namespace ArtClass.Application.Dtos;

public sealed record UpdateGroupRequest(
    string Name,
    string? Description,
    string Color,
    bool IsBiWeekly,
    IReadOnlyList<GroupSlotInput> Slots,
    IReadOnlyList<int> StudentIds);
