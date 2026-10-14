using System.ComponentModel.DataAnnotations;
using SmartToDo.Domain.Enums;

namespace SmartToDo.Application.DTOs;

public record UpdateTodoTaskRequest(
    [Required] string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate
);