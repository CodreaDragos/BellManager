namespace BellManager.Api.Models
{
	public class Church
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string PhoneNumber { get; set; } = string.Empty;
		public ICollection<User> Users { get; set; } = new List<User>();
		public ICollection<Alarm> Alarms { get; set; } = new List<Alarm>();
	}
}


