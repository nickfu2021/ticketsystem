using FluentValidation;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.validators;
public class CustomerUpdateDtoValidator : AbstractValidator<CustomerUpdateDto>
{
    public CustomerUpdateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("會員名稱不能空白");
        RuleFor(x => x.Email).NotEmpty().WithMessage("電子郵件不能空白");
    }
}