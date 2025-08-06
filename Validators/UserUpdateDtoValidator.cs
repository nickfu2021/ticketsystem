using System.Data;
using FluentValidation;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.validators;

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        RuleFor(x => x.Password).NotEmpty().WithMessage("密碼不能空白");
        RuleFor(x => x.Email).NotEmpty().WithMessage("電子信箱不能空白");
        RuleFor(x => x.Role).NotEmpty().WithMessage("請設定權限");
    }
}