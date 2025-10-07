using BellManager.Services;
using BellManager.Helpers;
using BellManager.Pages;

namespace BellManager.Pages;

public partial class RegisterPage : ContentPage
{
	private readonly AuthApiService _authService;

	public RegisterPage()
	{
		InitializeComponent();
		_authService = (AuthApiService)ServiceHelper.Services.GetService(typeof(AuthApiService))!;
	}

	private async void OnRegisterClicked(object sender, EventArgs e)
	{
		ErrorLabel.IsVisible = false;
		var email = EmailEntry.Text?.Trim() ?? string.Empty;
		var username = UsernameEntry.Text?.Trim() ?? string.Empty;
		var password = PasswordEntry.Text ?? string.Empty;
		if (!IsValidEmail(email))
		{
			ErrorLabel.Text = "Email not valid (a@b.cc).";
			ErrorLabel.IsVisible = true;
			return;
		}
		if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
		{
			ErrorLabel.Text = "Username must be at least 3 characters.";
			ErrorLabel.IsVisible = true;
			return;
		}
		if (password.Length < 8)
		{
			ErrorLabel.Text = "Password must be at least 8 characters.";
			ErrorLabel.IsVisible = true;
			return;
		}
		var (ok, error) = await _authService.RegisterAsync(email, username, password);
		if (!ok)
		{
			ErrorLabel.Text = string.IsNullOrWhiteSpace(error) ? "Registration failed." : error;
			ErrorLabel.IsVisible = true;
			return;
		}
		
		// Refresh admin tabs visibility after successful registration
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

	private async void OnLoginTapped(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//Login");
	}

	private static bool IsValidEmail(string email)
	{
		if (string.IsNullOrWhiteSpace(email)) return false;
		var at = email.IndexOf('@');
		var dot = email.LastIndexOf('.');
		return at > 0 && dot > at + 1 && dot < email.Length - 2;
	}
}


