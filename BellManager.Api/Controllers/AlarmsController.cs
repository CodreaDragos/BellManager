using BellManager.Api.Data;
using BellManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BellManager.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class AlarmsController : ControllerBase
	{
		private readonly AppDbContext _dbContext;

		public AlarmsController(AppDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		[HttpGet("today")]
		[AllowAnonymous]
		public async Task<ActionResult<IEnumerable<AlarmTodayDto>>> GetToday()
		{
			var today = DateTime.Now;
			var todayDate = DateOnly.FromDateTime(today);
			var dayOfWeekShort = today.DayOfWeek switch
			{
				DayOfWeek.Sunday => "Sun",
				DayOfWeek.Monday => "Mon",
				DayOfWeek.Tuesday => "Tue",
				DayOfWeek.Wednesday => "Wed",
				DayOfWeek.Thursday => "Thu",
				DayOfWeek.Friday => "Fri",
				DayOfWeek.Saturday => "Sat",
				_ => ""
			};

			// Get alarms that match either:
			// 1. DaysOfWeek contains today's day (for recurring alarms)
			// 2. SelectedDate matches today's date (for specific date alarms)
			var alarms = await _dbContext.Alarms
				.Include(a => a.Church)
				.Where(a => a.IsEnabled && 
					(a.DaysOfWeek.Contains(dayOfWeekShort) || 
					 (a.SelectedDate.HasValue && DateOnly.FromDateTime(a.SelectedDate.Value) == todayDate)))
				.AsNoTracking()
				.ToListAsync();

			var result = alarms
				.Select(a =>
				{
					// HourUtc stores the time as-is (despite the name, it's stored in local timezone)
					// Get phone number from Church, fallback to empty string if not available
					var phoneNumber = a.Church?.PhoneNumber ?? string.Empty;

					return new AlarmTodayDto
					{
						Ora = a.HourUtc.Hour,
						Minut = a.HourUtc.Minute,
						NumarApel = phoneNumber
					};
				})
				.OrderBy(a => a.Ora * 60 + a.Minut) // Sort by time
				.ToList();

			return Ok(result);
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Alarm>>> GetAll()
		{
			var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var alarms = await _dbContext.Alarms.Where(a => a.UserId == userId).AsNoTracking().ToListAsync();
			return Ok(alarms);
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<Alarm>> GetById(int id)
		{
			var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var alarm = await _dbContext.Alarms.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
			return alarm is null ? NotFound() : Ok(alarm);
		}

		[HttpPost]
		public async Task<ActionResult<Alarm>> Create([FromBody] Alarm alarm)
		{
			var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			alarm.UserId = userId;
			_dbContext.Alarms.Add(alarm);
			await _dbContext.SaveChangesAsync();
			return CreatedAtAction(nameof(GetById), new { id = alarm.Id }, alarm);
		}

		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, [FromBody] Alarm updated)
		{
			if (id != updated.Id) return BadRequest();
			var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var exists = await _dbContext.Alarms.AnyAsync(a => a.Id == id && a.UserId == userId);
			if (!exists) return NotFound();
			_dbContext.Entry(updated).State = EntityState.Modified;
			_dbContext.Entry(updated).Property(a => a.UserId).IsModified = false;
			await _dbContext.SaveChangesAsync();
			return NoContent();
		}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var alarm = await _dbContext.Alarms.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
			if (alarm is null) return NotFound();
			_dbContext.Alarms.Remove(alarm);
			await _dbContext.SaveChangesAsync();
			return NoContent();
		}
	}
}
