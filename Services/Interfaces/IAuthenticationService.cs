using StasDiplom.Domain;
using StasDiplom.Dto.Users.Login;
using StasDiplom.Dto.Users.Register;

namespace StasDiplom.Services.Interfaces;

public interface IAuthenticationService
{
    public Task<User> Register(RegisterRequest request);
    public Task<LoginResponse> Login(LoginRequest request);
}