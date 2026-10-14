using SmartToDo.Domain.Entities;

namespace SmartToDo.Domain.Interfaces;

public interface ITodoTaskRepository
{
    Task<TodoTask?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoTask>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(TodoTask task, CancellationToken cancellationToken = default);
    void Update(TodoTask task);
    void Delete(TodoTask task);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}