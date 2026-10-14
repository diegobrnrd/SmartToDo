using AutoMapper;
using Moq;
using SmartToDo.Application.DTOs;
using SmartToDo.Application.Interfaces;
using SmartToDo.Application.Mappings;
using SmartToDo.Application.Services;
using SmartToDo.Domain.Entities;
using SmartToDo.Domain.Enums;
using SmartToDo.Domain.Interfaces;

namespace SmartToDo.UnitTests.Application;

public class TodoTaskServiceTests
{
    private readonly Mock<ITodoTaskRepository> _taskRepositoryMock;
    private readonly Mock<IAiAssistantService> _aiAssistantServiceMock;
    private readonly IMapper _mapper;
    private readonly TodoTaskService _sut; // SUT = System Under Test

    public TodoTaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITodoTaskRepository>();
        _aiAssistantServiceMock = new Mock<IAiAssistantService>();

        // Usamos a configuração real do AutoMapper para validar também o nosso Profile nos testes
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TodoTaskMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _sut = new TodoTaskService(
            _taskRepositoryMock.Object,
            _aiAssistantServiceMock.Object,
            _mapper);
    }

    [Fact]
    public async Task CreateAsync_WhenAiSucceeds_ShouldEnrichTaskAndPersist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTodoTaskRequest(
            Title: "Estudar .NET 9 e Clean Architecture",
            Description: "Revisar padrões de repositório e testes com Moq",
            Priority: TaskPriority.High,
            DueDate: DateTime.UtcNow.AddDays(3));

        _aiAssistantServiceMock
            .Setup(ai => ai.AnalyzeTaskAsync(request.Title, request.Description))
            .ReturnsAsync(("Estudos", 60));

        // Act
        var response = await _sut.CreateAsync(request, userId);

        // Assert
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(request.Title, response.Title);
        Assert.Equal("Estudos", response.AiSuggestedCategory);
        Assert.Equal(60, response.AiEstimatedMinutes);

        _taskRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<TodoTask>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _taskRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenAiThrowsException_ShouldStillSaveTaskGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTodoTaskRequest(
            Title: "Fazer compras do mês",
            Description: "Ir ao supermercado",
            Priority: TaskPriority.Medium,
            DueDate: null);

        // Simula uma queda ou timeout na API da Inteligência Artificial
        _aiAssistantServiceMock
            .Setup(ai => ai.AnalyzeTaskAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ThrowsAsync(new HttpRequestException("Serviço de IA temporariamente indisponível."));

        // Act
        var response = await _sut.CreateAsync(request, userId);

        // Assert (Degradação Graciosa: a tarefa é criada normalmente sem os metadados de IA)
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Fazer compras do mês", response.Title);
        Assert.Null(response.AiSuggestedCategory);
        Assert.Null(response.AiEstimatedMinutes);

        _taskRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<TodoTask>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _taskRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoTask?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sut.GetByIdAsync(taskId, userId));

        Assert.Equal("Tarefa não encontrada.", exception.Message);
    }

    [Fact]
    public async Task CompleteAsync_WhenTaskExists_ShouldMarkAsCompletedAndUpdateRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingTask = new TodoTask(userId, "Finalizar testes da camada Application");

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(existingTask.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        // Act
        await _sut.CompleteAsync(existingTask.Id, userId);

        // Assert
        Assert.Equal(TodoTaskStatus.Completed, existingTask.Status);
        Assert.NotNull(existingTask.CompletedAt);

        _taskRepositoryMock.Verify(r => r.Update(existingTask), Times.Once);
        _taskRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateAiSubTasksAsync_WhenTaskExists_ShouldAddAiGeneratedSubTasksAndSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingTask = new TodoTask(userId, "Planejar viagem para a Europa", "Roteiro de 15 dias");

        var aiSteps = new List<string>
        {
            "Comprar passagens aéreas",
            "Reservar hotéis",
            "Contratar seguro viagem"
        };

        _taskRepositoryMock
            .Setup(r => r.GetByIdAsync(existingTask.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        _aiAssistantServiceMock
            .Setup(ai => ai.GenerateSubTasksAsync(existingTask.Title, existingTask.Description))
            .ReturnsAsync(aiSteps);

        // Act
        var response = await _sut.GenerateAiSubTasksAsync(existingTask.Id, userId);

        // Assert
        Assert.Equal(3, response.SubTasks.Count());
        Assert.All(response.SubTasks, subTask => Assert.True(subTask.IsAiGenerated));

        _taskRepositoryMock.Verify(r => r.Update(existingTask), Times.Once);
        _taskRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}