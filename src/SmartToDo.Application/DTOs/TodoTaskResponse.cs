using SmartToDo.Domain.Enums;

namespace SmartToDo.Application.DTOs;

public record SubTaskResponse(
    Guid Id,
    string Title,
    bool IsCompleted,
    bool IsAiGenerated
);

public record TodoTaskResponse(
    Guid Id,
    string Title,
    string? Description,
    TodoTaskStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? AiSuggestedCategory,
    int? AiEstimatedMinutes,
    IEnumerable<SubTaskResponse> SubTasks
);