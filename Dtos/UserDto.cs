namespace TicketSystemApi.Dtos;

public class UserDto
{
    public Guid UserUuid { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
}