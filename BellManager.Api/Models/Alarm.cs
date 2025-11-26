namespace BellManager.Api.Models
{
	public class Alarm
	{
		public int Id { get; set; }

		public int UserId { get; set; }

		public User? User { get; set; }

		public string BellName { get; set; } = string.Empty;

		// Store time in UTC to avoid timezone issues
		public TimeOnly HourUtc { get; set; }

		// Bit flags for days of week (0=Sunday ... 6=Saturday) or simple string like "Mon,Tue"
		// Bit flags for days of week (0=Sunday ... 6=Saturday) or simple string like "Mon,Tue"
		public List<string> DaysOfWeek { get; set; } = new List<string>();

		public bool IsEnabled { get; set; } = true;

		public string? Notes { get; set; }

		// New fields for enhanced repeat functionality
		public string RepeatType { get; set; } = "Once"; // "Once", "Sunday", "Custom"

		public bool IsRepeating { get; set; } = false; // Whether to repeat weekly

		public DateTime? SelectedDate { get; set; } // Specific date for custom alarms

		public int? ChurchId { get; set; } // Associated church

		public Church? Church { get; set; } // Navigation property
	}
}


