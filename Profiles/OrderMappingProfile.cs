using AutoMapper;
using TicketSystemApi.Dtos;
using TicketSystemApi.Models;

namespace TicketSystemApi.Profiles;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.EventName, opt => opt.MapFrom(src => src.Event.Name))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name));

        CreateMap<OrderCreateDto, Order>()
            .ForMember(dest => dest.Ordertime, opt => opt.MapFrom(src => DateTime.UtcNow));


        CreateMap<OrderUpdateDto, Order>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        /*
        CreateMap<OrderUpdateDto, Order>()
        建立從更新 DTO → 實體的對應規則。

        .ForAllMembers(...)
        這代表：套用到所有屬性（所有成員）。

        opt => opt.Condition(...)
        這是 AutoMapper 中用來「有條件地做屬性對應」的設定。

        (src, dest, srcMember) => srcMember != null
        這是條件判斷的 lambda 表達式，代表：

        如果來源屬性的值不是 null，才執行對應；如果是 null，就跳過這個欄位。
        */
    }

}