using FluentValidation;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.Validators;

public class OrderCreatedDtoValidator : AbstractValidator<OrderCreateDto>
{
    public OrderCreatedDtoValidator()
    {
        RuleFor(x => x.EventId).GreaterThan(0).WithMessage("活動 ID 必須大於 0");
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("客戶 ID 必須大於 0");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("數量必須大於 0");
    }
}