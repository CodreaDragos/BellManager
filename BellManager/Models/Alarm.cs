namespace BellManager.Models
{
	public class Alarm
	{
		public int Id { get; set; }
		public string BellName { get; set; } = string.Empty;
		public TimeOnly HourUtc { get; set; }
		public string DaysOfWeek { get; set; } = string.Empty;
		public bool IsEnabled { get; set; } = true;
		public string? Notes { get; set; }
		
		// New fields for enhanced repeat functionality
		public string RepeatType { get; set; } = "Once"; // "Once", "Sunday", "Custom"
		public bool IsRepeating { get; set; } = false; // Whether to repeat weekly
		public DateTime? SelectedDate { get; set; } // Specific date for custom alarms
		public int? ChurchId { get; set; } // Associated church
	}
}


