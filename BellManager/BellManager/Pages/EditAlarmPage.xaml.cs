using BellManager.Helpers;
using BellManager.ViewModels;
using System.Linq;
using BellManager.Services;

namespace BellManager.Pages
{
    public partial class EditAlarmPage : ContentPage
    {
        private readonly EditAlarmViewModel _viewModel;

        public EditAlarmPage()
        {
            InitializeComponent();
            _viewModel = ServiceHelper.GetRequiredService<EditAlarmViewModel>();
            BindingContext = _viewModel;
            
            // No need to populate pickers since we're using custom time picker
            
            // No need for event handler since we're using binding
            
            // No need for property change notifications since we use direct click events
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Always refresh church information when page appears
            await _viewModel.RefreshChurchInfoAsync();
            
            // Check if we have an alarm ID parameter for editing
            var currentLocation = Shell.Current.CurrentState.Location.OriginalString;
            if (currentLocation.Contains("alarmId="))
            {
                var queryString = currentLocation.Split('?').LastOrDefault();
                if (!string.IsNullOrEmpty(queryString))
                {
                    var queryParams = queryString.Split('&');
                    var alarmIdParam = queryParams.FirstOrDefault(p => p.StartsWith("alarmId="));
                    if (!string.IsNullOrEmpty(alarmIdParam))
                    {
                        var alarmIdStr = alarmIdParam.Split('=').LastOrDefault();
                        if (int.TryParse(alarmIdStr, out var alarmId))
                        {
                            // Load the alarm data for editing
                            var alarmService = ServiceHelper.GetRequiredService<Services.AlarmApiService>();
                            var alarms = await alarmService.GetAlarmsAsync();
                            var alarm = alarms.FirstOrDefault(a => a.Id == alarmId);
                            if (alarm != null)
                            {
                                _viewModel.LoadFrom(alarm);
                            }
                        }
                    }
                }
            }
            else
            {
                // For new alarms, reset the form
                _viewModel.LoadFrom(null);
            }
        }



        private void OnHourUpClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Hour Up clicked - current hour: {_viewModel.SelectedHour}");
            if (_viewModel.SelectedHour < 23)
            {
                _viewModel.SelectedHour++;
                System.Diagnostics.Debug.WriteLine($"Hour updated to: {_viewModel.SelectedHour}");
            }
        }

        private void OnHourDownClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Hour Down clicked - current hour: {_viewModel.SelectedHour}");
            if (_viewModel.SelectedHour > 0)
            {
                _viewModel.SelectedHour--;
                System.Diagnostics.Debug.WriteLine($"Hour updated to: {_viewModel.SelectedHour}");
            }
        }

        private void OnMinuteUpClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Minute Up clicked - current minute: {_viewModel.SelectedMinute}");
            if (_viewModel.SelectedMinute < 59)
            {
                _viewModel.SelectedMinute++;
                System.Diagnostics.Debug.WriteLine($"Minute updated to: {_viewModel.SelectedMinute}");
            }
            else
            {
                _viewModel.SelectedMinute = 0;
                if (_viewModel.SelectedHour < 23)
                {
                    _viewModel.SelectedHour++;
                    System.Diagnostics.Debug.WriteLine($"Minute wrapped, hour updated to: {_viewModel.SelectedHour}");
                }
            }
        }

        private void OnMinuteDownClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Minute Down clicked - current minute: {_viewModel.SelectedMinute}");
            if (_viewModel.SelectedMinute > 0)
            {
                _viewModel.SelectedMinute--;
                System.Diagnostics.Debug.WriteLine($"Minute updated to: {_viewModel.SelectedMinute}");
            }
            else
            {
                _viewModel.SelectedMinute = 59;
                if (_viewModel.SelectedHour > 0)
                {
                    _viewModel.SelectedHour--;
                    System.Diagnostics.Debug.WriteLine($"Minute wrapped, hour updated to: {_viewModel.SelectedHour}");
                }
            }
        }

        private void OnQuickTimeClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string timeString)
            {
                if (TimeOnly.TryParse(timeString, out var time))
                {
                    _viewModel.SelectedHour = time.Hour;
                    _viewModel.SelectedMinute = time.Minute;
                }
            }
        }
    }
}
