namespace BandHub.AuthService.Dtos;

public class LoginResultDto
{
    public string? AccessToken { get; set; }
    public int? ExpiresIn { get; set; }
    public string? UserName { get; set; }
};