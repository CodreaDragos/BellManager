using System.Net.Http.Json;

namespace BellManager.Services
{
	public class AdminUserApiService
	{
		private readonly HttpClient _httpClient;
		public AdminUserApiService(HttpClient httpClient) { _httpClient = httpClient; }

		public async Task<List<UserLite>> GetUsersAsync(CancellationToken ct = default)
			=> await _httpClient.GetFromJsonAsync<List<UserLite>>("api/users", ct) ?? new List<UserLite>();

		public async Task<bool> AssignChurchAsync(int userId, int churchId, CancellationToken ct = default)
		{
			var res = await _httpClient.PutAsync($"api/users/{userId}/assign-church/{churchId}", null, ct);
			return res.IsSuccessStatusCode;
		}

		public async Task<bool> RemoveChurchAssignmentAsync(int userId, CancellationToken ct = default)
		{
			var res = await _httpClient.DeleteAsync($"api/users/{userId}/assign-church", ct);
			return res.IsSuccessStatusCode;
		}

		public class UserLite
		{
			public int Id { get; set; }
			public string Email { get; set; } = string.Empty;
			public string UserName { get; set; } = string.Empty;
			public int? ChurchId { get; set; }
		}
	}
}


