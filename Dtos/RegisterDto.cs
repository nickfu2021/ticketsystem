namespace TicketSystemApi.Dtos;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? IdNumber { get; set; }
    public string? Birthday { get; set; }
    public string? MobileNumber { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? AddressDetail { get; set; }
}