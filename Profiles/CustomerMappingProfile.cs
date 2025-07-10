using TicketSystemApi.Dtos;
using TicketSystemApi.Models;

namespace TicketSystemApi.Profiles;

public class CustomerMappingProfile : AutoMapper.Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<CustomerCreateDto, Customer>();
        CreateMap<CustomerUpdateDto, Customer>();
    }
}