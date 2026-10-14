using Microsoft.Extensions.DependencyInjection;
using SmartToDo.Application.Interfaces;
using SmartToDo.Application.Mappings;
using SmartToDo.Application.Services;

namespace SmartToDo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registra todos os Profiles do AutoMapper presentes neste projeto
        services.AddAutoMapper(typeof(TodoTaskMappingProfile).Assembly);

        // Registra os serviços de aplicação com ciclo de vida Scoped (uma instância por requisição HTTP)
        services.AddScoped<ITodoTaskService, TodoTaskService>();

        return services;
    }
}