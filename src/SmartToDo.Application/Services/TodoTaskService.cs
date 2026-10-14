using AutoMapper;
using SmartToDo.Application.DTOs;
using SmartToDo.Application.Interfaces;
using SmartToDo.Domain.Entities;
using SmartToDo.Domain.Interfaces;

namespace SmartToDo.Application.Services;

public class TodoTaskService(
    ITodoTaskRepository taskRepository,
    IAiAssistantService aiAssistantService,
    IMapper mapper) : ITodoTaskService
{
    public async Task<TodoTaskResponse> CreateAsync(CreateTodoTaskRequest request, Guid userId)
    {
        // 1. Instancia a entidade rica (as validações de domínio ocorrem aqui)
        var task = new TodoTask(
            userId,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate);

        // 2. Enriquecimento via IA com Degradação Graciosa (não bloqueia a criação se a IA falhar)
        try
        {
            var (category, estimatedMinutes) = await aiAssistantService.AnalyzeTaskAsync(task.Title, task.Description);

            if (!string.IsNullOrWhiteSpace(category) || (estimatedMinutes.HasValue && estimatedMinutes.Value > 0))
            {
                var validMinutes = estimatedMinutes.HasValue && estimatedMinutes.Value > 0
                    ? estimatedMinutes
                    : null;

                task.ApplyAiEnrichment(category, validMinutes);
            }
        }
        catch
        {
            // Se o provedor de IA estiver indisponível, seguimos com a criação normal da tarefa
        }

        // 3. Persiste no banco de dados
        await taskRepository.AddAsync(task);
        await taskRepository.SaveChangesAsync();

        // 4. Converte a entidade para o DTO de resposta via AutoMapper
        return mapper.Map<TodoTaskResponse>(task);
    }

    public async Task<TodoTaskResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var task = await GetTaskOrThrowAsync(id, userId);
        return mapper.Map<TodoTaskResponse>(task);
    }

    public async Task<IEnumerable<TodoTaskResponse>> GetAllByUserIdAsync(Guid userId)
    {
        var tasks = await taskRepository.GetAllByUserIdAsync(userId);
        return mapper.Map<IEnumerable<TodoTaskResponse>>(tasks);
    }

    public async Task UpdateAsync(Guid id, Guid userId, UpdateTodoTaskRequest request)
    {
        var task = await GetTaskOrThrowAsync(id, userId);

        task.UpdateDetails(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate);

        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync();
    }

    public async Task CompleteAsync(Guid id, Guid userId)
    {
        var task = await GetTaskOrThrowAsync(id, userId);

        task.Complete();

        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var task = await GetTaskOrThrowAsync(id, userId);

        taskRepository.Delete(task);
        await taskRepository.SaveChangesAsync();
    }

    public async Task<TodoTaskResponse> GenerateAiSubTasksAsync(Guid id, Guid userId)
    {
        var task = await GetTaskOrThrowAsync(id, userId);

        var suggestedSteps = await aiAssistantService.GenerateSubTasksAsync(task.Title, task.Description);

        foreach (var step in suggestedSteps.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
            task.AddSubTask(step, isAiGenerated: true);
        }

        taskRepository.Update(task);
        await taskRepository.SaveChangesAsync();

        return mapper.Map<TodoTaskResponse>(task);
    }

    private async Task<TodoTask> GetTaskOrThrowAsync(Guid id, Guid userId)
    {
        var task = await taskRepository.GetByIdAsync(id, userId);
        if (task is null)
            throw new KeyNotFoundException("Tarefa não encontrada.");

        return task;
    }
}