namespace BellManager.Api.Models
{
	public class User
	{
		public int Id { get; set; }

		public string Email { get; set; } = string.Empty;

		public string UserName { get; set; } = string.Empty;

		public string PasswordHash { get; set; } = string.Empty;

		public string Role { get; set; } = "user";

		public int? ChurchId { get; set; }

		public Church? Church { get; set; }

		public ICollection<Alarm> Alarms { get; set; } = new List<Alarm>();
	}
}


