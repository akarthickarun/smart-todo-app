using AutoMapper;
using SmartTodoApp.Application.TodoItems.Commands.CreateTodoItem;
using SmartTodoApp.Application.TodoItems.Commands.UpdateTodoItem;
using SmartTodoApp.Domain.Entities;
using SmartTodoApp.Shared.Contracts.TodoItems;
using DomainTodoStatus = SmartTodoApp.Domain.Enums.TodoStatus;
using ContractTodoStatus = SmartTodoApp.Shared.Contracts.TodoItems.TodoStatus;

namespace SmartTodoApp.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for mapping between domain entities and DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Map TodoItem entity to TodoItemDto for API responses
        CreateMap<TodoItem, TodoItemDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (ContractTodoStatus)src.Status));

        // Map CreateTodoRequest to CreateTodoItemCommand for API layer
        CreateMap<CreateTodoRequest, CreateTodoItemCommand>()
            .ConstructUsing(src => new CreateTodoItemCommand(src.Title, src.Description, src.DueDate));

        // Explicit enum mapping to ensure alignment between domain and contract enums
        CreateMap<DomainTodoStatus, ContractTodoStatus>()
            .ConvertUsing(src => (ContractTodoStatus)src);
        CreateMap<ContractTodoStatus, DomainTodoStatus>()
            .ConvertUsing(src => (DomainTodoStatus)src);
    }
}
