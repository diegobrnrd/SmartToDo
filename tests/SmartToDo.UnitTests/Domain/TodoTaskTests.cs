using SmartToDo.Domain.Entities;
using SmartToDo.Domain.Enums;
using SmartToDo.Domain.Exceptions;

namespace SmartToDo.UnitTests.Domain;

public class TodoTaskTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreatePendingTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var title = "  Estudar Clean Architecture  ";
        var description = "Assistir aulas e praticar";
        var priority = TaskPriority.High;

        // Act
        var task = new TodoTask(userId, title, description, priority);

        // Assert
        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(userId, task.UserId);
        Assert.Equal("Estudar Clean Architecture", task.Title); // Garante que o Trim() funcionou
        Assert.Equal(description, task.Description);
        Assert.Equal(TaskPriority.High, task.Priority);
        Assert.Equal(TodoTaskStatus.Pending, task.Status);
        Assert.Null(task.CompletedAt);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldThrowDomainException()
    {
        // Arrange
        var emptyUserId = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            new TodoTask(emptyUserId, "Tarefa de teste"));

        Assert.Equal("A tarefa deve pertencer a um usuário válido.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrWhitespaceTitle_ShouldThrowDomainException(string? invalidTitle)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            new TodoTask(userId, invalidTitle!));

        Assert.Equal("O título da tarefa é obrigatório.", exception.Message);
    }

    [Fact]
    public void Constructor_WithTitleExceeding200Characters_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var longTitle = new string('A', 201);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            new TodoTask(userId, longTitle));

        Assert.Equal("O título da tarefa não pode exceder 200 caracteres.", exception.Message);
    }

    [Fact]
    public void Complete_WhenTaskIsPending_ShouldMarkAsCompletedAndSetCompletedAt()
    {
        // Arrange
        var task = new TodoTask(Guid.NewGuid(), "Gravar vídeo para o portfólio");

        // Act
        task.Complete();

        // Assert
        Assert.Equal(TodoTaskStatus.Completed, task.Status);
        Assert.NotNull(task.CompletedAt);
    }

    [Fact]
    public void StartProgress_WhenTaskIsAlreadyCompleted_ShouldThrowDomainException()
    {
        // Arrange
        var task = new TodoTask(Guid.NewGuid(), "Configurar Docker");
        task.Complete();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => task.StartProgress());
        Assert.Equal("Não é possível iniciar uma tarefa que já foi concluída.", exception.Message);
    }

    [Fact]
    public void Reopen_WhenTaskIsNotCompleted_ShouldThrowDomainException()
    {
        // Arrange
        var task = new TodoTask(Guid.NewGuid(), "Implementar JWT");

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => task.Reopen());
        Assert.Equal("Apenas tarefas concluídas podem ser reabertas.", exception.Message);
    }

    [Fact]
    public void ApplyAiEnrichment_WithValidData_ShouldSetCategoryAndEstimatedMinutes()
    {
        // Arrange
        var task = new TodoTask(Guid.NewGuid(), "Estudar algoritmos de grafos");

        // Act
        task.ApplyAiEnrichment("  Estudos  ", 45);

        // Assert
        Assert.Equal("Estudos", task.AiSuggestedCategory);
        Assert.Equal(45, task.AiEstimatedMinutes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-15)]
    public void ApplyAiEnrichment_WithZeroOrNegativeMinutes_ShouldThrowDomainException(int invalidMinutes)
    {
        // Arrange
        var task = new TodoTask(Guid.NewGuid(), "Estudar C#");

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            task.ApplyAiEnrichment("Programação", invalidMinutes));

        Assert.Equal("O tempo estimado deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public void AddSubTask_WithValidTitle_ShouldAddSubTaskToCollection()
    {
        // Arrange
        var task = new TodoTask(Guid.NewGuid(), "Planejar viagem");

        // Act
        var subTask = task.AddSubTask("Comprar passagens", isAiGenerated: true);

        // Assert
        Assert.Single(task.SubTasks);
        Assert.Equal("Comprar passagens", subTask.Title);
        Assert.True(subTask.IsAiGenerated);
        Assert.False(subTask.IsCompleted);
    }
}