using BellManager.Api.Data;
using BellManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BellManager.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = "admin")]
	public class UsersController : ControllerBase
	{
		private readonly AppDbContext _dbContext;

		public UsersController(AppDbContext dbContext)
		{
			_dbContext = dbContext;
		}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<UserLiteDto>>> GetAll()
	{
		var users = await _dbContext.Users
			.AsNoTracking()
			.Select(u => new UserLiteDto
			{
				Id = u.Id,
				Email = u.Email,
				UserName = u.UserName,
				ChurchId = u.ChurchId
			})
			.ToListAsync();
		return Ok(users);
	}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			var user = await _dbContext.Users.FindAsync(id);
			if (user is null) return NotFound();
			_dbContext.Users.Remove(user);
			await _dbContext.SaveChangesAsync();
			return NoContent();
		}

		[HttpPut("{id:int}/assign-church/{churchId:int}")]
		public async Task<IActionResult> AssignChurch(int id, int churchId)
		{
			var user = await _dbContext.Users.FindAsync(id);
			if (user is null) return NotFound("User not found");
			var church = await _dbContext.Churches.FindAsync(churchId);
			if (church is null) return NotFound("Church not found");
			user.ChurchId = churchId;
			await _dbContext.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id:int}/assign-church")]
		public async Task<IActionResult> RemoveChurchAssignment(int id)
		{
			var user = await _dbContext.Users.FindAsync(id);
			if (user is null) return NotFound("User not found");
			user.ChurchId = null;
			await _dbContext.SaveChangesAsync();
			return NoContent();
		}
	}

	public class UserLiteDto
	{
		public int Id { get; set; }
		public string Email { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public int? ChurchId { get; set; }
	}
}


