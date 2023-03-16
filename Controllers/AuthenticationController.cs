using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StasDiplom.Context;
using StasDiplom.Domain;
using StasDiplom.Dto.Users.Login;
using StasDiplom.Dto.Users.Register;
using StasDiplom.Services.Interfaces;
using StasDiplom.Utility;

namespace StasDiplom.Controllers;

[ApiController]
[Route("api/account/")]
public class AuthenticationController : Controller
{
    private readonly IAuthenticationService _userLogRegService;
    private readonly ICalendarService _calendarService;

    public AuthenticationController(IAuthenticationService userLogRegService, ICalendarService calendarService)
    {
        _userLogRegService = userLogRegService;
        _calendarService = calendarService;
    }

    [HttpPost("register")]
    [ProducesResponseType(200)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        User user;
        try
        {
            user = await _userLogRegService.Register(request);
        }
        catch (Exception e)
        {
            return e switch
            {
                InvalidOperationException or ArgumentException => Conflict(),
                _ => StatusCode(500)
            };
        }

        await _calendarService.CreateUserCalendar(user);
        return Ok();
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            return Ok(await _userLogRegService.Login(request));
        }
        catch (InvalidOperationException)
        {
            return BadRequest();
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
    }
}