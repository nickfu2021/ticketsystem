using System.Data;
using FluentValidation;
using TicketSystemApi.Data;
using TicketSystemApi.Dtos;
using Microsoft.EntityFrameworkCore;
using TicketSystemApi.Services;

namespace TicketSystemApi.validators;

public class GroupCreateDtoValidator : AbstractValidator<GroupCreateDto>
{
    public GroupCreateDtoValidator()
    {
        RuleFor(g => g.Name)
            .NotEmpty().WithMessage("群組代號不可空白")
            .MaximumLength(4).WithMessage("群組代號最多4碼");
        RuleFor(g => g.Description)
            .NotEmpty().WithMessage("群組名稱不可空白");
    }           
}