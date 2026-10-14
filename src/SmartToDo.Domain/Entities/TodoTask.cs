using SmartToDo.Domain.Enums;
using SmartToDo.Domain.Exceptions;

namespace SmartToDo.Domain.Entities;

public class TodoTask : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TodoTaskStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Metadados enriquecidos pela Inteligência Artificial
    public string? AiSuggestedCategory { get; private set; }
    public int? AiEstimatedMinutes { get; private set; }

    private readonly List<SubTask> _subTasks = new();
    public IReadOnlyCollection<SubTask> SubTasks => _subTasks.AsReadOnly();

    protected TodoTask() { }

    public TodoTask(
        Guid userId,
        string title,
        string? description = null,
        TaskPriority priority = TaskPriority.Medium,
        DateTime? dueDate = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("A tarefa deve pertencer a um usuário válido.");

        ValidateTitle(title);

        UserId = userId;
        Title = title.Trim();
        Description = description?.Trim();
        Priority = priority;
        Status = TodoTaskStatus.Pending;
        DueDate = dueDate;
    }

    public void UpdateDetails(string title, string? description, TaskPriority priority, DateTime? dueDate)
    {
        ValidateTitle(title);

        Title = title.Trim();
        Description = description?.Trim();
        Priority = priority;
        DueDate = dueDate;
        MarkAsUpdated();
    }

    public void StartProgress()
    {
        if (Status == TodoTaskStatus.Completed)
            throw new DomainException("Não é possível iniciar uma tarefa que já foi concluída.");

        Status = TodoTaskStatus.InProgress;
        MarkAsUpdated();
    }

    public void Complete()
    {
        if (Status == TodoTaskStatus.Completed)
            return;

        Status = TodoTaskStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Reopen()
    {
        if (Status != TodoTaskStatus.Completed)
            throw new DomainException("Apenas tarefas concluídas podem ser reabertas.");

        Status = TodoTaskStatus.Pending;
        CompletedAt = null;
        MarkAsUpdated();
    }

    public void ApplyAiEnrichment(string? category, int? estimatedMinutes)
    {
        if (estimatedMinutes.HasValue && estimatedMinutes.Value <= 0)
            throw new DomainException("O tempo estimado deve ser maior que zero.");

        AiSuggestedCategory = category?.Trim();
        AiEstimatedMinutes = estimatedMinutes;
        MarkAsUpdated();
    }

    public SubTask AddSubTask(string title, bool isAiGenerated = false)
    {
        var subTask = new SubTask(Id, title, isAiGenerated);
        _subTasks.Add(subTask);
        MarkAsUpdated();
        return subTask;
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("O título da tarefa é obrigatório.");

        if (title.Trim().Length > 200)
            throw new DomainException("O título da tarefa não pode exceder 200 caracteres.");
    }
}