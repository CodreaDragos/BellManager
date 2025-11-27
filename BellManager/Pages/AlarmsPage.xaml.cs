using BellManager.ViewModels;
using BellManager.Helpers;
using BellManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using System;

namespace BellManager.Pages
{
    public partial class AlarmsPage : ContentPage
    {
        private readonly AlarmsViewModel _viewModel;
        private System.Timers.Timer _clockTimer;

        public AlarmsPage()
        {
            InitializeComponent();
            _viewModel = ServiceHelper.GetRequiredService<AlarmsViewModel>();
            BindingContext = _viewModel;

            // Start the clock timer
            StartClockTimer();
        }

        private void StartClockTimer()
        {
            _clockTimer = new System.Timers.Timer(1000); // Update every second
            _clockTimer.Elapsed += (sender, e) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateTimeLabel();
                });
            };
            _clockTimer.Start();

            // Set initial time
            UpdateTimeLabel();
            
            // Subscribe to culture changes
            LocalizationResourceManager.Instance.PropertyChanged += (s, e) =>
            {
                UpdateTimeLabel();
            };
        }
        
        private void UpdateTimeLabel()
        {
            if (CurrentTimeLabel != null)
            {
                var culture = BellManager.Resources.Strings.AppResources.Culture ?? System.Globalization.CultureInfo.CurrentCulture;
                CurrentTimeLabel.Text = DateTime.Now.ToString("dddd, MMMM dd • h:mm tt", culture);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("=== AlarmsPage.OnAppearing START ===");
            
            // First, try to attach any saved authentication token
            var authService = ServiceHelper.GetRequiredService<AuthApiService>();
            var tokenAttached = await authService.TryAttachSavedTokenAsync();
            System.Diagnostics.Debug.WriteLine($"Token attached: {tokenAttached}");
            
            // Check if user is authenticated before loading alarms
            var userService = ServiceHelper.GetRequiredService<UserService>();
            var user = await userService.GetCurrentUserAsync();
            
            if (user != null)
            {
                System.Diagnostics.Debug.WriteLine($"User authenticated: {user.Username}, loading alarms...");
                await _viewModel.LoadAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No authenticated user, skipping alarm load");
            }
            
            System.Diagnostics.Debug.WriteLine("=== AlarmsPage.OnAppearing END ===");
            _clockTimer?.Start();
        }

        public async Task InitializeAsync()
        {
            System.Diagnostics.Debug.WriteLine("=== AlarmsPage.InitializeAsync START ===");
            
            // First, try to attach any saved authentication token
            var authService = ServiceHelper.GetRequiredService<AuthApiService>();
            var tokenAttached = await authService.TryAttachSavedTokenAsync();
            System.Diagnostics.Debug.WriteLine($"Token attached: {tokenAttached}");
            
            // Check if user is authenticated before loading alarms
            var userService = ServiceHelper.GetRequiredService<UserService>();
            var user = await userService.GetCurrentUserAsync();
            
            if (user != null)
            {
                System.Diagnostics.Debug.WriteLine($"User authenticated: {user.Username}, loading alarms...");
                await _viewModel.LoadAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No authenticated user, skipping alarm load");
            }
            
            System.Diagnostics.Debug.WriteLine("=== AlarmsPage.InitializeAsync END ===");
        }

        public void TriggerOnAppearing()
        {
            OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _clockTimer?.Stop();
        }

        public async Task RefreshAlarmsAsync()
        {
            System.Diagnostics.Debug.WriteLine("=== AlarmsPage.RefreshAlarmsAsync START ===");
            await _viewModel.LoadAsync();
            System.Diagnostics.Debug.WriteLine("=== AlarmsPage.RefreshAlarmsAsync END ===");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            var auth = ServiceHelper.GetRequiredService<AuthApiService>();
            await auth.LogoutAsync();
            
            // Hide admin tabs after logout
            if (Shell.Current is AppShell appShell)
            {
                await appShell.RefreshAdminTabsAsync();
            }
            
            await Shell.Current.GoToAsync("/Login");
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            // Clean up timer when page is disposed
            if (BindingContext == null)
            {
                _clockTimer?.Stop();
                _clockTimer?.Dispose();
            }
        }
    }
}