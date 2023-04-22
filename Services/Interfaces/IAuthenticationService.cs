using TaskService.Domain;
using TaskService.Dto.Users.Login;
using TaskService.Dto.Users.Register;

namespace TaskService.Services.Interfaces;

public interface IAuthenticationService
{
    public Task<User> Register(RegisterRequest request);
    public Task<LoginResponse> Login(LoginRequest request);
}