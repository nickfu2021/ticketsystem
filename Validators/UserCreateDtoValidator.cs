using System.Data;
using FluentValidation;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.validators;

public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
{
    public UserCreateDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("名稱不能空白");
        RuleFor(x => x.Password).NotEmpty().WithMessage("密碼不能空白");
        RuleFor(x => x.Email).NotEmpty().WithMessage("電子信箱不能空白");
    }
}