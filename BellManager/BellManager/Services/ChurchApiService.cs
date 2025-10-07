using System.Net.Http.Json;
using BellManager.Models;

namespace BellManager.Services
{
	public class ChurchApiService
	{
		private readonly HttpClient _httpClient;
		public ChurchApiService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

	public async Task<List<Church>> GetAllAsync(CancellationToken ct = default)
	{
		try
		{
			System.Diagnostics.Debug.WriteLine("Making API call to api/churches");
			var response = await _httpClient.GetAsync("api/churches", ct);
			System.Diagnostics.Debug.WriteLine($"API response status: {response.StatusCode}");
			
			if (!response.IsSuccessStatusCode)
			{
				var errorContent = await response.Content.ReadAsStringAsync();
				System.Diagnostics.Debug.WriteLine($"API error: {errorContent}");
				return new List<Church>();
			}
			
			var churches = await response.Content.ReadFromJsonAsync<List<Church>>(ct);
			System.Diagnostics.Debug.WriteLine($"API returned {churches?.Count ?? 0} churches");
			return churches ?? new List<Church>();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error in GetAllAsync: {ex.Message}");
			return new List<Church>();
		}
	}

		public async Task<Church?> CreateAsync(Church church, CancellationToken ct = default)
		{
			var res = await _httpClient.PostAsJsonAsync("api/churches", church, ct);
			if (!res.IsSuccessStatusCode) return null;
			return await res.Content.ReadFromJsonAsync<Church>(cancellationToken: ct);
		}

		public async Task<bool> UpdateAsync(Church church, CancellationToken ct = default)
		{
			var res = await _httpClient.PutAsJsonAsync($"api/churches/{church.Id}", church, ct);
			return res.IsSuccessStatusCode;
		}

		public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
		{
			var res = await _httpClient.DeleteAsync($"api/churches/{id}", ct);
			return res.IsSuccessStatusCode;
		}
	}
}


