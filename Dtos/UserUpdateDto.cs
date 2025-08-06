namespace TicketSystemApi.Dtos;

public class UserUpdateDto
{
    public int Id { get; set; }
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}