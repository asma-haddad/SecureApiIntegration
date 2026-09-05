using ExpenseAuthApi.Data;
using ExpenseAuthApi.DTOs;
using ExpenseAuthApi.Exceptions;
using ExpenseAuthApi.Model;
using ExpenseAuthApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthController(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var emailExists = await _context.Users
            .AnyAsync(x => x.Email == request.Email);

        if (emailExists)
            throw new ConflictException("Email already exists");

        var user = new User
        {
            Email = request.Email,
            Role = "User"
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email);

        if (user == null)
            throw new UnAuthorizedException("Invalid email or password");

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (result == PasswordVerificationResult.Failed)
            throw new UnAuthorizedException("Invalid email or password");

        var tokens =
            await _tokenService.IssueTokensAsync(
                user.Id,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString());

        return Ok(tokens);
    }
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
    RefreshTokenRequest request)
    {
        var tokens = await _tokenService.RefreshAsync(
            request.RefreshToken,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString());

        return Ok(tokens);
    }
}