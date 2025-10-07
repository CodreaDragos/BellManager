using System.Net.Http.Json;
using System.Net.Http.Headers;
using BellManager.Models;

namespace BellManager.Services
{
	public class AlarmApiService
	{
		private readonly HttpClient _httpClient;

		public AlarmApiService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public void SetBearerToken(string? token)
		{
			if (string.IsNullOrWhiteSpace(token))
			{
				_httpClient.DefaultRequestHeaders.Authorization = null;
			}
			else
			{
				_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			}
		}

		public async Task<List<Alarm>> GetAlarmsAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				var result = await _httpClient.GetFromJsonAsync<List<Alarm>>("api/alarms", cancellationToken);
				return result ?? new List<Alarm>();
			}
			catch
			{
				return new List<Alarm>();
			}
		}

		public async Task<Alarm?> CreateAsync(Alarm alarm, CancellationToken cancellationToken = default)
		{
			try
			{
				System.Diagnostics.Debug.WriteLine($"Creating alarm: {System.Text.Json.JsonSerializer.Serialize(alarm)}");
				var response = await _httpClient.PostAsJsonAsync("api/alarms", alarm, cancellationToken);
				System.Diagnostics.Debug.WriteLine($"API Response Status: {response.StatusCode}");
				
				if (!response.IsSuccessStatusCode) 
				{
					var errorContent = await response.Content.ReadAsStringAsync();
					System.Diagnostics.Debug.WriteLine($"API Error: {errorContent}");
					return null;
				}
				
				var result = await response.Content.ReadFromJsonAsync<Alarm>(cancellationToken: cancellationToken);
				System.Diagnostics.Debug.WriteLine($"Created alarm: {System.Text.Json.JsonSerializer.Serialize(result)}");
				return result;
			}
			catch (Exception ex) 
			{ 
				System.Diagnostics.Debug.WriteLine($"CreateAsync Exception: {ex.Message}");
				return null; 
			}
		}

		public async Task<bool> UpdateAsync(Alarm alarm, CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await _httpClient.PutAsJsonAsync($"api/alarms/{alarm.Id}", alarm, cancellationToken);
				return response.IsSuccessStatusCode;
			}
			catch { return false; }
		}

		public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await _httpClient.DeleteAsync($"api/alarms/{id}", cancellationToken);
				return response.IsSuccessStatusCode;
			}
			catch { return false; }
		}
	}
}
