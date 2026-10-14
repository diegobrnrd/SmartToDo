using SmartToDo.Application.DTOs;

namespace SmartToDo.Application.Interfaces;

public interface ITodoTaskService
{
    Task<TodoTaskResponse> CreateAsync(CreateTodoTaskRequest request, Guid userId);
    Task<TodoTaskResponse> GetByIdAsync(Guid id, Guid userId);
    Task<IEnumerable<TodoTaskResponse>> GetAllByUserIdAsync(Guid userId);
    Task UpdateAsync(Guid id, Guid userId, UpdateTodoTaskRequest request);
    Task CompleteAsync(Guid id, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);

    // O endpoint "mágico" que aciona a IA sob demanda para uma tarefa existente
    Task<TodoTaskResponse> GenerateAiSubTasksAsync(Guid id, Guid userId);
}