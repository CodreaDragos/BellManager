using System.Net.Http.Json;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace BellManager.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private const string UserInfoKey = "user_info";

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserInfo?> GetCurrentUserAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== GetCurrentUserAsync START ===");
                // Always try to get fresh data from API first
                var response = await _httpClient.GetAsync("api/auth/me");
                System.Diagnostics.Debug.WriteLine($"API response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>();
                    System.Diagnostics.Debug.WriteLine($"API returned user: {userInfo?.Username}, ChurchId: {userInfo?.ChurchId}");
                    
                    if (userInfo != null)
                    {
                        await StoreUserAsync(userInfo);
                        System.Diagnostics.Debug.WriteLine("Stored user info in secure storage");
                        return userInfo;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"API error: {errorContent}");
                }

                // If API call fails, try to get from secure storage
                var storedUser = await GetStoredUserAsync();
                System.Diagnostics.Debug.WriteLine($"Stored user: {storedUser?.Username}, ChurchId: {storedUser?.ChurchId}");
                if (storedUser != null)
                {
                    return storedUser;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in GetCurrentUserAsync: {ex.Message}");
                // If API call fails, try to get from secure storage
                var storedUser = await GetStoredUserAsync();
                System.Diagnostics.Debug.WriteLine($"Stored user after exception: {storedUser?.Username}, ChurchId: {storedUser?.ChurchId}");
                if (storedUser != null)
                {
                    return storedUser;
                }
            }

            System.Diagnostics.Debug.WriteLine("=== GetCurrentUserAsync END - returning null ===");
            return null;
        }

        public async Task<bool> IsAdminAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.Role == "admin";
        }

        public async Task ClearUserAsync()
        {
            SecureStorage.Default.Remove(UserInfoKey);
        }

        private async Task<UserInfo?> GetStoredUserAsync()
        {
            try
            {
                var userJson = await SecureStorage.Default.GetAsync(UserInfoKey);
                if (string.IsNullOrEmpty(userJson))
                    return null;

                return JsonSerializer.Deserialize<UserInfo>(userJson);
            }
            catch
            {
                return null;
            }
        }

        private async Task StoreUserAsync(UserInfo userInfo)
        {
            try
            {
                var userJson = JsonSerializer.Serialize(userInfo);
                await SecureStorage.Default.SetAsync(UserInfoKey, userJson);
            }
            catch
            {
                // Ignore storage errors
            }
        }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? ChurchId { get; set; }
    }
}
