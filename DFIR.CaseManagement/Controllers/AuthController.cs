using System.Security.Claims;
using DFIR.CaseManagement.Auth;
using DFIR.CaseManagement.DTOs;
using DFIR.CaseManagement.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFIR.CaseManagement.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IValidator<RegisterDto> _registerValidator;

    public AuthController(IAuthService auth, IValidator<RegisterDto> registerValidator)
    {
        _auth = auth;
        _registerValidator = registerValidator;
    }

    /// <summary>Authenticate and receive an access + refresh token pair.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _auth.AuthenticateAsync(request);
        return result is null ? Unauthorized(new { message = "Invalid username or password." }) : Ok(result);
    }

    /// <summary>Exchange a valid refresh token for a new token pair.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Refresh([FromBody] RefreshRequestDto request)
    {
        var result = await _auth.RefreshAsync(request);
        return result is null ? Unauthorized(new { message = "Invalid or expired refresh token." }) : Ok(result);
    }

    /// <summary>Create a new user. Requires the users:manage permission (Admin).</summary>
    [HttpPost("register")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto)
    {
        var validation = await _registerValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var user = await _auth.RegisterAsync(dto);
        return user is null ? Conflict(new { message = "Username already exists." }) : Ok(user);
    }

    [HttpGet("users")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetUsers()
        => Ok(await _auth.GetUsersAsync());

    [HttpDelete("users/{id:int}")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> DeleteUser(int id)
        => await _auth.DeleteUserAsync(id) ? NoContent() : NotFound();

    /// <summary>Returns the identity / claims of the currently authenticated user.</summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> Me()
    {
        return Ok(new
        {
            username = User.Identity?.Name,
            role = User.FindFirstValue(ClaimTypes.Role),
            permissions = User.FindAll(AppClaimTypes.Permission).Select(c => c.Value).ToArray()
        });
    }
}
