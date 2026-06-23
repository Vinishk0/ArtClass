using ArtClass.Application.Caching;
using ArtClass.Application.Data;
using ArtClass.Application.Dtos;
using ArtClass.Domain.Entities;
using ArtClass.Domain.Repositories;

namespace ArtClass.Application.Services;

public sealed class GroupService(UnitOfWorkExecutor db, IAppDataCache cache) : IGroupService
{
    public async Task<IReadOnlyList<GroupDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string key = "groups:all";
        if (cache.TryGet(key, out IReadOnlyList<GroupDto>? cached) && cached is not null)
        {
            return cached;
        }

        var groups = await db.QueryAsync(
            (unitOfWork, ct) => unitOfWork.StudyGroups.GetAllWithDetailsAsync(ct),
            cancellationToken);
        var mapped = groups.Select(MapGroup).ToList();
        cache.Set(key, mapped);
        return mapped;
    }

    public async Task<GroupDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var key = $"group:{id}";
        if (cache.TryGet(key, out GroupDto? cached))
        {
            return cached;
        }

        var group = await db.QueryAsync(
            (unitOfWork, ct) => unitOfWork.StudyGroups.GetByIdWithDetailsAsync(id, ct),
            cancellationToken);
        if (group is null)
        {
            return null;
        }

        var mapped = MapGroup(group);
        cache.Set(key, mapped);
        return mapped;
    }

    public async Task<int> CreateRepeatingGroupAsync(
        CreateGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var groupId = await db.QueryAsync(async (unitOfWork, ct) =>
        {
            var refs = await ResolveReferenceIdsAsync(unitOfWork, ct);

            var group = new StudyGroup
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                Color = GroupColors.Normalize(request.Color),
                IsRepeating = true,
                IsBiWeekly = request.IsBiWeekly,
            };

            await unitOfWork.StudyGroups.AddAsync(group, ct);
            await unitOfWork.SaveChangesAsync(ct);

            await AddLessonsAsync(unitOfWork, group.Id, refs, request.Slots, request.IsBiWeekly, ct);
            await SyncEnrollmentsAsync(unitOfWork, group.Id, request.StudentIds, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return group.Id;
        }, cancellationToken);

        cache.InvalidateGroups();
        return groupId;
    }

    public async Task UpdateRepeatingGroupAsync(
        int groupId,
        UpdateGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        await db.ExecuteAsync(async (unitOfWork, ct) =>
        {
            var group = await unitOfWork.StudyGroups.GetByIdForUpdateAsync(groupId, ct)
                ?? throw new InvalidOperationException("Группа не найдена");

            if (!group.IsRepeating)
            {
                throw new InvalidOperationException("Редактирование доступно только для групп с повтором");
            }

            var refs = await ResolveReferenceIdsAsync(unitOfWork, ct);

            group.Name = request.Name.Trim();
            group.Description = request.Description?.Trim();
            group.Color = GroupColors.Normalize(request.Color);
            group.IsBiWeekly = request.IsBiWeekly;

            await unitOfWork.Lessons.DeleteByStudyGroupIdAsync(groupId, ct);
            await unitOfWork.StudyGroups.UpdateAsync(group, ct);
            await AddLessonsAsync(unitOfWork, groupId, refs, request.Slots, request.IsBiWeekly, ct);
            await SyncEnrollmentsAsync(unitOfWork, groupId, request.StudentIds, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        cache.InvalidateGroups();
    }

    public async Task UpdateExtraLessonAsync(
        int groupId,
        UpdateExtraLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        await db.ExecuteAsync(async (unitOfWork, ct) =>
        {
            var group = await unitOfWork.StudyGroups.GetByIdForUpdateAsync(groupId, ct)
                ?? throw new InvalidOperationException("Мастеркласс не найден");

            if (group.IsRepeating)
            {
                throw new InvalidOperationException("Редактирование доступно только для мастерклассов");
            }

            var refs = await ResolveReferenceIdsAsync(unitOfWork, ct);

            group.Name = request.Name.Trim();
            group.Description = string.IsNullOrWhiteSpace(request.Notes)
                ? "Разовое занятие"
                : request.Notes.Trim();
            group.Color = GroupColors.Normalize(request.Color);

            await unitOfWork.Lessons.DeleteByStudyGroupIdAsync(groupId, ct);
            await unitOfWork.Lessons.AddAsync(new Lesson
            {
                StudyGroupId = group.Id,
                TeacherId = refs.TeacherId,
                SubjectId = refs.SubjectId,
                ClassroomId = refs.ClassroomId,
                DayOfWeek = request.Date.DayOfWeek,
                SpecificDate = request.Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Notes = request.Notes,
            }, ct);

            await unitOfWork.StudyGroups.UpdateAsync(group, ct);
            await SyncEnrollmentsAsync(unitOfWork, groupId, request.StudentIds, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        cache.InvalidateGroups();
    }

    public async Task<int> CreateExtraLessonAsync(
        CreateExtraLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        var groupId = await db.QueryAsync(async (unitOfWork, ct) =>
        {
            var refs = await ResolveReferenceIdsAsync(unitOfWork, ct);

            var group = new StudyGroup
            {
                Name = request.Name.Trim(),
                Description = "Разовое занятие",
                Color = GroupColors.Normalize(request.Color),
                IsRepeating = false,
            };

            await unitOfWork.StudyGroups.AddAsync(group, ct);
            await unitOfWork.SaveChangesAsync(ct);

            await unitOfWork.Lessons.AddAsync(new Lesson
            {
                StudyGroupId = group.Id,
                TeacherId = refs.TeacherId,
                SubjectId = refs.SubjectId,
                ClassroomId = refs.ClassroomId,
                DayOfWeek = request.Date.DayOfWeek,
                SpecificDate = request.Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Notes = request.Notes,
            }, ct);

            await SyncEnrollmentsAsync(unitOfWork, group.Id, request.StudentIds, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return group.Id;
        }, cancellationToken);

        cache.InvalidateGroups();
        return groupId;
    }

    public async Task DeleteAsync(int groupId, CancellationToken cancellationToken = default)
    {
        await db.ExecuteAsync(async (unitOfWork, ct) =>
        {
            var group = await unitOfWork.StudyGroups.GetByIdForUpdateAsync(groupId, ct)
                ?? throw new InvalidOperationException("Группа не найдена");

            await unitOfWork.Lessons.DeleteByStudyGroupIdAsync(groupId, ct);
            unitOfWork.StudyGroups.Remove(group);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        cache.InvalidateGroups();
    }

    private static async Task AddLessonsAsync(
        IUnitOfWork unitOfWork,
        int groupId,
        ReferenceIds refs,
        IReadOnlyList<GroupSlotInput> slots,
        bool isBiWeekly,
        CancellationToken cancellationToken)
    {
        foreach (var slot in slots)
        {
            await unitOfWork.Lessons.AddAsync(new Lesson
            {
                StudyGroupId = groupId,
                TeacherId = refs.TeacherId,
                SubjectId = refs.SubjectId,
                ClassroomId = refs.ClassroomId,
                DayOfWeek = slot.DayOfWeek,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                CycleWeek = isBiWeekly ? slot.CycleWeek : null,
            }, cancellationToken);
        }
    }

    private static async Task SyncEnrollmentsAsync(
        IUnitOfWork unitOfWork,
        int groupId,
        IReadOnlyList<int> studentIds,
        CancellationToken cancellationToken)
    {
        var group = await unitOfWork.StudyGroups.GetByIdForUpdateAsync(groupId, cancellationToken)
            ?? throw new InvalidOperationException("Группа не найдена");

        var targetIds = studentIds.Distinct().ToHashSet();
        var currentIds = group.StudentEnrollments.Select(e => e.StudentId).ToHashSet();

        foreach (var studentId in currentIds.Except(targetIds))
        {
            await unitOfWork.Students.UnenrollAsync(studentId, groupId, cancellationToken);
        }

        foreach (var studentId in targetIds.Except(currentIds))
        {
            await unitOfWork.Students.EnrollAsync(studentId, groupId, cancellationToken);
        }
    }

    private static async Task<ReferenceIds> ResolveReferenceIdsAsync(
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var teacher = (await unitOfWork.Teachers.GetAllAsync(cancellationToken)).FirstOrDefault()
            ?? throw new InvalidOperationException("В базе нет преподавателей");
        var subject = (await unitOfWork.Subjects.GetAllAsync(cancellationToken)).FirstOrDefault()
            ?? throw new InvalidOperationException("В базе нет предметов");
        var classroom = (await unitOfWork.Classrooms.GetAllAsync(cancellationToken)).FirstOrDefault()
            ?? throw new InvalidOperationException("В базе нет кабинетов");

        return new ReferenceIds(teacher.Id, subject.Id, classroom.Id);
    }

    private static GroupDto MapGroup(StudyGroup group) =>
        new(
            group.Id,
            group.Name,
            group.Description,
            GroupColors.Normalize(group.Color),
            group.IsRepeating,
            group.IsBiWeekly,
            group.Lessons
                .OrderBy(l => l.CycleWeek)
                .ThenBy(l => l.DayOfWeek)
                .ThenBy(l => l.StartTime)
                .Select(l => new GroupSlotDto(
                    l.Id,
                    l.DayOfWeek,
                    l.StartTime,
                    l.EndTime,
                    l.CycleWeek,
                    l.SpecificDate))
                .ToList(),
            group.StudentEnrollments.Select(e => e.StudentId).ToList(),
            group.StudentEnrollments.Count);

    private sealed record ReferenceIds(int TeacherId, int SubjectId, int ClassroomId);
}
