using BellManager.Services;
using BellManager.Helpers;
using BellManager.Pages;

namespace BellManager.Pages;

public partial class LoginPage : ContentPage
{
	private readonly AuthApiService _authService;

	public LoginPage()
	{
		InitializeComponent();
		_authService = (AuthApiService)ServiceHelper.Services.GetService(typeof(AuthApiService))!;
	}

	private async void OnLoginClicked(object sender, EventArgs e)
	{
		try
		{
			ErrorLabel.IsVisible = false;
			var user = UsernameEntry.Text?.Trim() ?? string.Empty;
			var pass = PasswordEntry.Text ?? string.Empty;
			if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
			{
				ErrorLabel.Text = "Enter credentials";
				ErrorLabel.IsVisible = true;
				return;
			}
			
			System.Diagnostics.Debug.WriteLine($"Attempting login for user: {user}");
			var (ok, error) = await _authService.LoginAsync(user, pass);
			System.Diagnostics.Debug.WriteLine($"Login result: {ok}, Error: {error}");
			
			if (!ok)
			{
				ErrorLabel.Text = string.IsNullOrWhiteSpace(error) ? "Wrong email or password" : error;
				ErrorLabel.IsVisible = true;
				return;
			}
			
			// Refresh admin tabs visibility after successful login
			if (Shell.Current is AppShell appShell)
			{
				await appShell.RefreshAdminTabsAsync();
			}
			
			await Shell.Current.GoToAsync("//Alarms");
			
			// Add a small delay to ensure the page is fully loaded, then refresh alarms
			await Task.Delay(500);
			if (Shell.Current.CurrentPage is AlarmsPage alarmsPage)
			{
				// Refresh the alarms page
				await alarmsPage.RefreshAlarmsAsync();
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
			ErrorLabel.Text = $"Login failed: {ex.Message}";
			ErrorLabel.IsVisible = true;
		}
	}

	private async void OnRegisterClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new RegisterPage());
	}

	public void OnEnglishClicked(object sender, EventArgs e)
	{
		BellManager.Helpers.LocalizationResourceManager.Instance.SetCulture(new System.Globalization.CultureInfo("en"));
	}

	public void OnRomanianClicked(object sender, EventArgs e)
	{
		BellManager.Helpers.LocalizationResourceManager.Instance.SetCulture(new System.Globalization.CultureInfo("ro"));
	}
}


