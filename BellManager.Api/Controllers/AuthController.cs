using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BellManager.Api.Data;
using BellManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BellManager.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly AppDbContext _dbContext;
		private readonly JwtSettings _jwtSettings;

		public AuthController(AppDbContext dbContext, IOptions<JwtSettings> jwtOptions)
		{
			_dbContext = dbContext;
			_jwtSettings = jwtOptions.Value;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequest request)
		{
			if (!IsValidEmail(request.Email)) return BadRequest("Email is not valid.");
			if (string.IsNullOrWhiteSpace(request.UserName) || request.UserName.Length < 3) return BadRequest("Username must be at least 3 characters.");
			if (request.Password.Length < 8) return BadRequest("Password must be at least 8 characters.");

			var emailInUse = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);
			if (emailInUse) return Conflict("Email already in use.");
			var usernameInUse = await _dbContext.Users.AnyAsync(u => u.UserName == request.UserName);
			if (usernameInUse) return Conflict("Username already in use.");

			var user = new User
			{
				Email = request.Email,
				UserName = request.UserName,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
				Role = "user"
			};
			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			var token = GenerateJwt(user);
			return Ok(new AuthResponse { Token = token, User = new UserDto(user.Id, user.Email, user.UserName, user.Role, user.ChurchId) });
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == request.EmailOrUserName || u.UserName == request.EmailOrUserName);
			if (user is null) return Unauthorized("Invalid credentials.");
			if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return Unauthorized("Invalid credentials.");

			var token = GenerateJwt(user);
			return Ok(new AuthResponse { Token = token, User = new UserDto(user.Id, user.Email, user.UserName, user.Role, user.ChurchId) });
		}

		[Authorize]
		[HttpGet("me")]
		public async Task<IActionResult> Me()
		{
			var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();
			var user = await _dbContext.Users.FindAsync(userId);
			if (user is null) return Unauthorized();
			return Ok(new UserDto(user.Id, user.Email, user.UserName, user.Role, user.ChurchId));
		}

		[Authorize]
		[HttpGet("my-church")]
		public async Task<IActionResult> GetMyChurch()
		{
			var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();
			
			var user = await _dbContext.Users.FindAsync(userId);
			if (user is null) return Unauthorized();
			
			if (user.ChurchId == null) return Ok((object?)null);
			
			var church = await _dbContext.Churches.FindAsync(user.ChurchId);
			if (church is null) return Ok((object?)null);
			
			return Ok(new { Id = church.Id, Name = church.Name, PhoneNumber = church.PhoneNumber });
		}

		private string GenerateJwt(User user)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Role, user.Role)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var token = new JwtSecurityToken(
				issuer: _jwtSettings.Issuer,
				audience: _jwtSettings.Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddDays(7),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		private static bool IsValidEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email)) return false;
			// Very simple validation: chars@chars.dot with 2+ TLD
			try
			{
				var at = email.IndexOf('@');
				var dot = email.LastIndexOf('.');
				return at > 0 && dot > at + 1 && dot < email.Length - 2;
			}
			catch { return false; }
		}
	}

	public record RegisterRequest(string Email, string UserName, string Password);
	public record LoginRequest(string EmailOrUserName, string Password);
	public record UserDto(int Id, string Email, string UserName, string Role, int? ChurchId = null);
	public record AuthResponse
	{
		public string Token { get; set; } = string.Empty;
		public UserDto? User { get; set; }
	}

	public class JwtSettings
	{
		public string Secret { get; set; } = string.Empty;
		public string Issuer { get; set; } = "bellmanager-api";
		public string Audience { get; set; } = "bellmanager-client";
	}
}


