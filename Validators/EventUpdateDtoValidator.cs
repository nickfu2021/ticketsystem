using FluentValidation;
using TicketSystemApi.Dtos;

namespace TicketSystemApi.Validators;

public class EventUpdateDtoValidator : AbstractValidator<EventUpdateDto>
{
    public EventUpdateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("活動名稱不能為空");
        RuleFor(x => x.Location).NotEmpty().WithMessage("活動地點不能為空");
    }
}