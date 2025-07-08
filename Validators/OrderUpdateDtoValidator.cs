using FluentValidation;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.Validators;

public class OrderUpdateDtoValidator : AbstractValidator<OrderUpdateDto>
{
    public OrderUpdateDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("訂單 ID 必須大於 0");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("數量必須大於 0");
    }
}