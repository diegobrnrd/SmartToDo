using SmartToDo.Domain.Exceptions;

namespace SmartToDo.Domain.Entities;

public class SubTask : BaseEntity
{
    public Guid TodoTaskId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public bool IsCompleted { get; private set; }
    public bool IsAiGenerated { get; private set; }

    protected SubTask() { }

    public SubTask(Guid todoTaskId, string title, bool isAiGenerated = false)
    {
        if (todoTaskId == Guid.Empty)
            throw new DomainException("A subtarefa deve estar vinculada a uma tarefa válida.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("O título da subtarefa é obrigatório.");

        TodoTaskId = todoTaskId;
        Title = title.Trim();
        IsCompleted = false;
        IsAiGenerated = isAiGenerated;
    }

    public void ToggleCompletion()
    {
        IsCompleted = !IsCompleted;
        MarkAsUpdated();
    }
}