namespace TaskService.Dto.Users.Login;

public class LoginResponse
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string Token { get; set; }
}