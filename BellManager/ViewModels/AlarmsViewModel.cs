using System.Collections.ObjectModel;
using System.Windows.Input;
using BellManager.Models;
using BellManager.Services;
using BellManager.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BellManager.ViewModels
{
    public class AlarmsViewModel : INotifyPropertyChanged
    {
        private readonly AlarmApiService _apiService;

        public ObservableCollection<AlarmDisplayModel> Alarms { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand ItemTappedCommand { get; }
        public ICommand DeleteCommand { get; }

        public AlarmsViewModel(AlarmApiService apiService)
        {
            _apiService = apiService;
            RefreshCommand = new Command(async () => await LoadAsync());
            AddCommand = new Command(async () => await AddAsync());
            ItemTappedCommand = new Command<AlarmDisplayModel?>(async a => await EditAsync(a));
            DeleteCommand = new Command<AlarmDisplayModel?>(async a => await DeleteAsync(a));
        }

        public async Task LoadAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== AlarmsViewModel.LoadAsync START ===");
                var items = await _apiService.GetAlarmsAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded {items.Count} alarms from API");
                Alarms.Clear();
                foreach (var item in items)
                {
                    Alarms.Add(new AlarmDisplayModel(item));
                }
                System.Diagnostics.Debug.WriteLine($"Added {Alarms.Count} alarms to UI collection");
                System.Diagnostics.Debug.WriteLine("=== AlarmsViewModel.LoadAsync END ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AlarmsViewModel.LoadAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // swallow to avoid crashing UI on emulator connectivity issues
            }
        }

        private async Task AddAsync()
        {
            await Shell.Current.GoToAsync("EditAlarm");
        }

        private async Task EditAsync(AlarmDisplayModel? alarmDisplay)
        {
            if (alarmDisplay?.OriginalAlarm == null) return;

            // Pass the alarm ID as a parameter to the EditAlarm page
            await Shell.Current.GoToAsync($"EditAlarm?alarmId={alarmDisplay.OriginalAlarm.Id}");
        }

        private async Task DeleteAsync(AlarmDisplayModel? alarmDisplay)
        {
            if (alarmDisplay?.OriginalAlarm == null) return;

            var ok = await _apiService.DeleteAsync(alarmDisplay.OriginalAlarm.Id);
            if (ok)
            {
                var match = Alarms.FirstOrDefault(a => a.OriginalAlarm.Id == alarmDisplay.OriginalAlarm.Id);
                if (match != null) Alarms.Remove(match);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Display model wrapper for the Alarm to provide UI-specific properties
    public class AlarmDisplayModel : INotifyPropertyChanged
    {
        private readonly Alarm _originalAlarm;

        public Alarm OriginalAlarm => _originalAlarm;

        public AlarmDisplayModel(Alarm alarm)
        {
            _originalAlarm = alarm;

            // Subscribe to property changes if the original alarm implements INotifyPropertyChanged
            if (_originalAlarm is INotifyPropertyChanged notifyAlarm)
            {
                notifyAlarm.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
            }
        }

        // Delegate basic properties from the original alarm
        public string BellName => _originalAlarm.BellName ?? "Alarm";

        public bool IsEnabled
        {
            get => _originalAlarm.IsEnabled;
            set
            {
                if (_originalAlarm.IsEnabled != value)
                {
                    _originalAlarm.IsEnabled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusColor));
                    
                    // Save the change to the database
                    _ = Task.Run(async () => await SaveToggleChangeAsync());
                }
            }
        }
        
        private async Task SaveToggleChangeAsync()
        {
            try
            {
                var apiService = ServiceHelper.GetRequiredService<Services.AlarmApiService>();
                await apiService.UpdateAsync(_originalAlarm);
                System.Diagnostics.Debug.WriteLine($"Alarm {_originalAlarm.Id} toggle saved: {_originalAlarm.IsEnabled}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving alarm toggle: {ex.Message}");
            }
        }

        public string HourUtc => _originalAlarm.HourUtc.ToString("HH:mm");

        // New UI-specific properties
        public string TimeDisplay
        {
            get
            {
                return _originalAlarm.HourUtc.ToString("h:mm");
            }
        }

        public string AmPmDisplay
        {
            get
            {
                return _originalAlarm.HourUtc.ToString("tt").ToLower();
            }
        }


        public bool HasBellName => !string.IsNullOrWhiteSpace(_originalAlarm.BellName);

        public string RepeatInfo
        {
            get
            {
                // Check if your Alarm model has these repeat properties
                // If not, you might need to add them or modify this logic
                try
                {
                    var repeatDaily = GetPropertyValue<bool>("RepeatDaily");
                    var repeatWeekly = GetPropertyValue<bool>("RepeatWeekly");

                    if (repeatDaily) return "Every day";

                    if (repeatWeekly)
                    {
                        var days = new List<string>();
                        if (GetPropertyValue<bool>("RepeatSun")) days.Add("Sun");
                        if (GetPropertyValue<bool>("RepeatMon")) days.Add("Mon");
                        if (GetPropertyValue<bool>("RepeatTue")) days.Add("Tue");
                        if (GetPropertyValue<bool>("RepeatWed")) days.Add("Wed");
                        if (GetPropertyValue<bool>("RepeatThu")) days.Add("Thu");
                        if (GetPropertyValue<bool>("RepeatFri")) days.Add("Fri");
                        if (GetPropertyValue<bool>("RepeatSat")) days.Add("Sat");

                        if (days.Count == 7) return "Every day";
                        if (days.Count == 5 && !GetPropertyValue<bool>("RepeatSat") && !GetPropertyValue<bool>("RepeatSun"))
                            return "Weekdays";
                        if (days.Count == 2 && GetPropertyValue<bool>("RepeatSat") && GetPropertyValue<bool>("RepeatSun"))
                            return "Weekends";
                        return days.Count > 0 ? string.Join(", ", days) : "Once";
                    }
                }
                catch
                {
                    // If repeat properties don't exist, fall back to "Once"
                }

                return "Once";
            }
        }

        public bool HasRepeatInfo => RepeatInfo != "Once";

        public Color StatusColor => IsEnabled ? Color.FromArgb("#4CAF50") : Color.FromArgb("#9E9E9E");

        // Helper method to get property values using reflection
        private T GetPropertyValue<T>(string propertyName)
        {
            var property = _originalAlarm.GetType().GetProperty(propertyName);
            if (property != null)
            {
                var value = property.GetValue(_originalAlarm);
                if (value is T typedValue)
                    return typedValue;
            }
            return default(T);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}