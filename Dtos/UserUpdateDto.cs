namespace TicketSystemApi.Dtos;

public class UserUpdateDto
{
    public Guid UserUuid { get; set; }
    public string Password { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Birthday { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string AddressDetail { get; set; } = string.Empty;
}