using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Maui.Storage;

namespace BellManager.Services
{
	public class AuthApiService
	{
		private readonly HttpClient _httpClient;

		private const string TokenKey = "auth_token";

		public AuthApiService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<(bool ok, string? error)> RegisterAsync(string email, string username, string password, CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.PostAsJsonAsync("api/auth/register", new { Email = email, UserName = username, Password = password }, cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				var msg = await response.Content.ReadAsStringAsync(cancellationToken);
				return (false, NormalizeMessage(msg));
			}
			var res = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
			if (res is null || string.IsNullOrWhiteSpace(res.Token)) return (false, "Unknown error");
			await SaveTokenAsync(res.Token);
			await StoreUserInfoAsync(res.User);
			return (true, null);
		}

		public async Task<(bool ok, string? error)> LoginAsync(string emailOrUsername, string password, CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { EmailOrUserName = emailOrUsername, Password = password }, cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				var msg = await response.Content.ReadAsStringAsync(cancellationToken);
				return (false, NormalizeMessage(msg));
			}
			var res = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
			if (res is null || string.IsNullOrWhiteSpace(res.Token)) return (false, "Unknown error");
			await SaveTokenAsync(res.Token);
			await StoreUserInfoAsync(res.User);
			return (true, null);
		}

		public async Task LogoutAsync()
		{
			SecureStorage.Default.Remove(TokenKey);
			SecureStorage.Default.Remove("user_info");
			_httpClient.DefaultRequestHeaders.Authorization = null;
		}

		public async Task<bool> TryAttachSavedTokenAsync()
		{
			var token = await SecureStorage.Default.GetAsync(TokenKey);
			if (string.IsNullOrWhiteSpace(token)) return false;
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			return true;
		}

		private async Task SaveTokenAsync(string token)
		{
			await SecureStorage.Default.SetAsync(TokenKey, token);
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}

		private async Task StoreUserInfoAsync(UserDto? user)
		{
			if (user != null)
			{
				var userInfo = new UserInfo
				{
					Id = user.Id,
					Email = user.Email,
					Username = user.UserName,
					Role = user.Role
				};
				var userJson = System.Text.Json.JsonSerializer.Serialize(userInfo);
				await SecureStorage.Default.SetAsync("user_info", userJson);
			}
		}

		private class AuthResponse
		{
			public string? Token { get; set; }
			public UserDto? User { get; set; }
		}

		private class UserDto
		{
			public int Id { get; set; }
			public string Email { get; set; } = string.Empty;
			public string UserName { get; set; } = string.Empty;
			public string Role { get; set; } = string.Empty;
		}

		private class UserInfo
		{
			public int Id { get; set; }
			public string Email { get; set; } = string.Empty;
			public string Username { get; set; } = string.Empty;
			public string Role { get; set; } = string.Empty;
		}

		private static string NormalizeMessage(string msg)
		{
			if (string.IsNullOrWhiteSpace(msg)) return "Request failed";
			msg = msg.Trim('"');
			return msg;
		}
	}
}


