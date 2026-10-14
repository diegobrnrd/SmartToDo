using System.ComponentModel.DataAnnotations;
using SmartToDo.Domain.Enums;

namespace SmartToDo.Application.DTOs;

public record CreateTodoTaskRequest(
    [Required(ErrorMessage = "O título é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O título não pode exceder 200 caracteres.")]
    string Title,

    string? Description,
    TaskPriority Priority,
    DateTime? DueDate
);