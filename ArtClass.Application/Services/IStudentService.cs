using ArtClass.Application.Dtos;
using ArtClass.Domain.Repositories;

namespace ArtClass.Application.Services;

public interface IStudentService
{
    Task<IReadOnlyList<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<StudentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(int studentId, UpdateStudentRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int studentId, CancellationToken cancellationToken = default);
}
