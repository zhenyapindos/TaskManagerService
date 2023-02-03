using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StasDiplom.Domain;
using StasDiplom.Dto.Users.Login;

namespace StasDiplom;

public class UserController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
        
    public UserController(UserManager<User> userManager, SignInManager<User> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }
    
    [HttpPost("register")]
    [ProducesResponseType(200)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        var userExists = await _userManager.FindByNameAsync(model.Username);

        if (userExists != null) return Conflict();

        User user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }
    
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);

        if (user == null) return NotFound();

        var result = await _signInManager.PasswordSignInAsync(
            user: user,
            password: request.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.IsLockedOut) return Forbid();

        if (!result.Succeeded) return BadRequest();

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(MyClaims.Id, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JWToken(_configuration);
        var resultToken = token.GetToken(authClaims);
        
        return Ok(new LoginResponse()
        {
            
            Token = new JwtSecurityTokenHandler().WriteToken(resultToken)
        });
    }
}