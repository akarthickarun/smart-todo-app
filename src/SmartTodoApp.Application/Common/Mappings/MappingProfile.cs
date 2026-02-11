using AutoMapper;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Shared.Contracts.TodoItems;

namespace SmartTodoApp.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for mapping between domain entities and DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Map TodoItem entity to TodoItemDto for API responses
        CreateMap<TodoItem, TodoItemDto>();
    }
}
