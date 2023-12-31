﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TaskService.Context;
using TaskService.Domain;
using TaskService.Dto.Users.Login;
using TaskService.Dto.Users.Register;
using TaskService.Services.Interfaces;
using TaskService.Utility;

namespace TaskService.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthenticationService(UserManager<User> userManager, SignInManager<User> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    public async Task<User> Register(RegisterRequest request)
    {
        var userExists = await _userManager.FindByNameAsync(request.Username);

        if (userExists != null)
        {
            throw new ArgumentException();
        }

        userExists = await _userManager.FindByEmailAsync(request.Email);

        if (userExists != null) 
        {
            throw new ArgumentException();
        }
        
        User user = new()
        {
            Email = request.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName
        };
        
        var result = await _userManager.CreateAsync(user, request.Password);

        if (result == null)
        {
            throw new InvalidOperationException();
        }

        return user;
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var user = request.EmailOrUsername.Contains('@')
            ? await _userManager.FindByEmailAsync(request.EmailOrUsername)
            : await _userManager.FindByNameAsync(request.EmailOrUsername);

        if (user == null)
        {
            throw new ArgumentException();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user: user,
            password: request.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException();
        }

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(MyClaims.Id, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JWToken(_configuration);
        var resultToken = token.GetToken(authClaims);

        return new LoginResponse()
        {
            Username = user.UserName,
            Email = user.Email,
            Token = new JwtSecurityTokenHandler().WriteToken(resultToken)
        };
    }
}