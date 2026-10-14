using AutoMapper;
using SmartToDo.Application.DTOs;
using SmartToDo.Domain.Entities;

namespace SmartToDo.Application.Mappings;

public class TodoTaskMappingProfile : Profile
{
    public TodoTaskMappingProfile()
    {
        // Mapeia a entidade SubTask para o record SubTaskResponse
        CreateMap<SubTask, SubTaskResponse>();

        // Mapeia a entidade TodoTask (incluindo a lista interna de SubTasks) para TodoTaskResponse
        CreateMap<TodoTask, TodoTaskResponse>();
    }
}