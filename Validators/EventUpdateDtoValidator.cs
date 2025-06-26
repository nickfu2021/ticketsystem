using FluentValidation;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.Validators;

public class EventUpdateDtoValidator : AbstractValidator<EventUpdateDto>
{
    public EventUpdateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("活動名稱不能為空");
        RuleFor(x => x.Location).NotEmpty().WithMessage("活動地點不能為空");
        RuleFor(x => x.EventDate).GreaterThanOrEqualTo(DateTime.Today).WithMessage("活動日期不能小於今日");
        RuleFor(x => x.TotalTickets).GreaterThan(0).WithMessage("總票數不能小於0");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("票價不得為負數");
    }
}