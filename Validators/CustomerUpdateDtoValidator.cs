using FluentValidation;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.validators;

public class CustomerUpdateDtoValidator : AbstractValidator<CustomerUpdateDto>
{
    public CustomerUpdateDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("會員 ID 必須大於 0");
        RuleFor(x => x.Name).NotEmpty().WithMessage("會員名稱不能空白");
        RuleFor(x => x.Email).NotEmpty().WithMessage("電子郵件不能空白");
    }
}