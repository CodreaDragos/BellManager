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
	public class ChurchesController : ControllerBase
	{
		private readonly AppDbContext _db;

		public ChurchesController(AppDbContext db)
		{
			_db = db;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Church>>> GetAll()
		{
			return Ok(await _db.Churches.AsNoTracking().ToListAsync());
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<Church>> GetById(int id)
		{
			var church = await _db.Churches.FindAsync(id);
			return church is null ? NotFound() : Ok(church);
		}

		[HttpPost]
		public async Task<ActionResult<Church>> Create([FromBody] Church church)
		{
			_db.Churches.Add(church);
			await _db.SaveChangesAsync();
			return CreatedAtAction(nameof(GetById), new { id = church.Id }, church);
		}

		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, [FromBody] Church updated)
		{
			if (id != updated.Id) return BadRequest();
			var exists = await _db.Churches.AnyAsync(c => c.Id == id);
			if (!exists) return NotFound();
			_db.Entry(updated).State = EntityState.Modified;
			await _db.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			var church = await _db.Churches.FindAsync(id);
			if (church is null) return NotFound();
			_db.Churches.Remove(church);
			await _db.SaveChangesAsync();
			return NoContent();
		}
	}
}


