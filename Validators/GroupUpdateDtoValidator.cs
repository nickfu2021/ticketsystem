using System.Data;
using FluentValidation;
using TicketSystemApi.Data;
using TicketSystemApi.Dtos;
using Microsoft.EntityFrameworkCore;
using TicketSystemApi.Services;

namespace TicketSystemApi.validators;

public class GroupUpdateDtoValidator : AbstractValidator<GroupCreateDto>
{
    public GroupUpdateDtoValidator()
    {
        RuleFor(g => g.Description)
            .NotEmpty().WithMessage("群組名稱不可空白");
    }           
}