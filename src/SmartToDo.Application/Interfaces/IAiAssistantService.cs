namespace SmartToDo.Application.Interfaces;

public interface IAiAssistantService
{
    /// <summary>
    /// Analisa o título e a descrição da tarefa para sugerir uma categoria e o tempo estimado em minutos.
    /// </summary>
    Task<(string? SuggestedCategory, int? EstimatedMinutes)> AnalyzeTaskAsync(string title, string? description);

    /// <summary>
    /// Quebra uma tarefa complexa em passos menores e acionáveis (Sub-tarefas).
    /// </summary>
    Task<List<string>> GenerateSubTasksAsync(string title, string? description);
}