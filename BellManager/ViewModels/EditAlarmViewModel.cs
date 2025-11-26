using System.Windows.Input;
using BellManager.Models;
using BellManager.Services;
using BellManager.Helpers;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BellManager.ViewModels
{
    public enum RepeatType
    {
        Once,
        Sunday,
        Custom
    }
    [QueryProperty(nameof(AlarmId), "alarmId")]
    public class EditAlarmViewModel : INotifyPropertyChanged
	{
		private readonly AlarmApiService _api;
		private readonly UserService _userService;

        public string AlarmId
        {
            set
            {
                if (int.TryParse(value, out var id))
                {
                    System.Diagnostics.Debug.WriteLine($"EditAlarmViewModel: Received AlarmId {id}");
                    _ = LoadAlarmAsync(id);
                }
            }
        }



		public int Id { get; set; }
        public string BellName { get; set; } = string.Empty;
        
        // Church information
        private string _churchName = string.Empty;
        public string ChurchName 
        { 
            get => _churchName; 
            set => SetProperty(ref _churchName, value); 
        }
        
        private string _churchPhone = string.Empty;
        public string ChurchPhone 
        { 
            get => _churchPhone; 
            set => SetProperty(ref _churchPhone, value); 
        }
        
        private bool _hasChurch = false;
        public bool HasChurch 
        { 
            get => _hasChurch; 
            set => SetProperty(ref _hasChurch, value); 
        }
        
        private int? _churchId = null;
        public int? ChurchId 
        { 
            get => _churchId; 
            set => SetProperty(ref _churchId, value); 
        }
        // Time selection via pickers
        private TimeSpan _selectedTime = new TimeSpan(7, 30, 0);
        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                if (SetProperty(ref _selectedTime, value))
                {
                    // Sync legacy properties
                    _selectedHour = value.Hours;
                    _selectedMinute = value.Minutes;
                    OnPropertyChanged(nameof(SelectedHour));
                    OnPropertyChanged(nameof(SelectedMinute));
                }
            }
        }

        private int _selectedHour = 7;
        public int SelectedHour 
        { 
            get => _selectedHour; 
            set 
            {
                if (SetProperty(ref _selectedHour, value))
                {
                    // Sync TimeSpan
                    if (_selectedTime.Hours != value)
                    {
                        SelectedTime = new TimeSpan(value, _selectedMinute, 0);
                    }
                }
            } 
        }
        
        private int _selectedMinute = 30;
        public int SelectedMinute 
        { 
            get => _selectedMinute; 
            set 
            {
                if (SetProperty(ref _selectedMinute, value))
                {
                    // Sync TimeSpan
                    if (_selectedTime.Minutes != value)
                    {
                        SelectedTime = new TimeSpan(_selectedHour, value, 0);
                    }
                }
            } 
        }

        // New repeat system
        private RepeatType _selectedRepeatType = RepeatType.Once;
        public RepeatType SelectedRepeatType 
        { 
            get => _selectedRepeatType; 
            set 
            { 
                System.Diagnostics.Debug.WriteLine($"SelectedRepeatType changing from {_selectedRepeatType} to {value} - StackTrace: {Environment.StackTrace}");
                if (SetProperty(ref _selectedRepeatType, value))
                {
                    System.Diagnostics.Debug.WriteLine($"IsCustomSelected: {IsCustomSelected}");
                    OnPropertyChanged(nameof(IsOnceSelected));
                    OnPropertyChanged(nameof(IsSundaySelected));
                    OnPropertyChanged(nameof(IsCustomSelected));
                    OnPropertyChanged(nameof(RepeatExplanation));
                    // Don't notify SelectedRepeatTypeIndex to avoid circular dependency
                }
            } 
        }
        
        // Index-based property for Picker binding
        public int SelectedRepeatTypeIndex
        {
            get => (int)_selectedRepeatType;
            set
            {
                System.Diagnostics.Debug.WriteLine($"SelectedRepeatTypeIndex setter called with value: {value}, current type: {_selectedRepeatType}");
                if (value >= 0 && value < Enum.GetValues<RepeatType>().Length)
                {
                    var newType = (RepeatType)value;
                    if (newType != _selectedRepeatType)
                    {
                        System.Diagnostics.Debug.WriteLine($"SelectedRepeatTypeIndex changing from {(int)_selectedRepeatType} to {value} - StackTrace: {Environment.StackTrace}");
                        SelectedRepeatType = newType;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"SelectedRepeatTypeIndex: No change needed, already {newType}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"SelectedRepeatTypeIndex: Invalid value {value}, ignoring");
                }
            }
        }
        
        private bool _isRepeating = false;
        public bool IsRepeating 
        { 
            get => _isRepeating; 
            set 
            { 
                if (SetProperty(ref _isRepeating, value))
                {
                    OnPropertyChanged(nameof(CustomExplanation));
                    OnPropertyChanged(nameof(RepeatExplanation));
                }
            } 
        }
        
        // Custom day selection (only used when RepeatType is Custom)
        public bool RepeatSun { get; set; } = false;
        public bool RepeatMon { get; set; } = false;
        public bool RepeatTue { get; set; } = false;
        public bool RepeatWed { get; set; } = false;
        public bool RepeatThu { get; set; } = false;
        public bool RepeatFri { get; set; } = false;
        public bool RepeatSat { get; set; } = false;

        // Date picker for specific date selection
        private DateTime? _selectedDate;
        public DateTime? SelectedDate 
        { 
            get => _selectedDate; 
            set 
            { 
                if (SetProperty(ref _selectedDate, value))
                {
                    OnPropertyChanged(nameof(SelectedDateText));
                    OnPropertyChanged(nameof(HasSelectedDate));
                    
                    // If a specific date is selected, disable repeating since it's a one-time event
                    if (value.HasValue && IsRepeating)
                    {
                        IsRepeating = false;
                    }
                }
            } 
        }
        
        public string SelectedDateText => SelectedDate?.ToString("ddd, MMM dd") ?? "No date selected";
        public bool HasSelectedDate => SelectedDate.HasValue;
        
        public DateTime Today => DateTime.Today;
        public DateTime MaxDate => DateTime.Today.AddYears(1);

        public List<string> DaysOfWeek { get; set; } = new List<string>();
		public bool IsEnabled { get; set; } = true;
		public string? Notes { get; set; }

        // Properties for XAML bindings
        public List<string> RepeatTypeOptions => new() { "Once", "Sunday", "Custom" };
        
        public bool IsOnceSelected => SelectedRepeatType == RepeatType.Once;
        public bool IsSundaySelected => SelectedRepeatType == RepeatType.Sunday;
        public bool IsCustomSelected => SelectedRepeatType == RepeatType.Custom;
        
        public string CustomExplanation => IsRepeating 
            ? "This alarm will repeat every week on the selected days" 
            : "This alarm will ring once on the next occurrence of the selected day";
            
        public string RepeatExplanation
        {
            get
            {
                if (IsRepeating)
                {
                    return SelectedRepeatType switch
                    {
                        RepeatType.Once => "This alarm will repeat every week on the same day",
                        RepeatType.Sunday => "This alarm will repeat every Sunday",
                        RepeatType.Custom => "This alarm will repeat every week on the selected days",
                        _ => "This alarm will repeat every week"
                    };
                }
                else
                {
                    return SelectedRepeatType switch
                    {
                        RepeatType.Once => "This alarm will ring once at the specified time (Today if time hasn't passed, otherwise tomorrow)",
                        RepeatType.Sunday => "This alarm will ring once on the next Sunday",
                        RepeatType.Custom => "This alarm will ring once on the next occurrence of the selected day",
                        _ => "This alarm will ring once"
                    };
                }
            }
        }

		public ICommand SaveCommand { get; }
		public ICommand OpenDatePickerCommand { get; }
		public ICommand ClearDateCommand { get; }

		public EditAlarmViewModel(AlarmApiService api, UserService userService)
		{
			_api = api;
			_userService = userService;
			SaveCommand = new Command(async () => await SaveAsync());
			OpenDatePickerCommand = new Command(async () => await OpenDatePickerAsync());
			ClearDateCommand = new Command(() => SelectedDate = null);
			
			// Load church information when ViewModel is created
			_ = Task.Run(async () => await LoadChurchInfoAsync());
		}

        private async Task LoadAlarmAsync(int id)
        {
            try
            {
                var alarm = await _api.GetAsync(id);
                if (alarm != null)
                {
                    MainThread.BeginInvokeOnMainThread(() => LoadFrom(alarm));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading alarm {id}: {ex.Message}");
            }
        }

        public void LoadFrom(Alarm? alarm)
        {
            if (alarm == null) 
            {
                // For new alarms, reset to defaults
                Id = 0;
                BellName = string.Empty;
                SelectedHour = 7;
                SelectedMinute = 30;
                SelectedTime = new TimeSpan(7, 30, 0);
                SelectedRepeatType = RepeatType.Once;
                IsRepeating = false;
                RepeatSun = false;
                RepeatMon = false;
                RepeatTue = false;
                RepeatWed = false;
                RepeatThu = false;
                RepeatFri = false;
                RepeatSat = false;
                DaysOfWeek = new List<string>();
                IsEnabled = true;
                Notes = null;
                OnPropertyChanged(nameof(SelectedRepeatTypeIndex));
                return;
            }
            
            Id = alarm.Id;
            BellName = alarm.BellName;

            SelectedHour = alarm.HourUtc.Hour;
            SelectedMinute = alarm.HourUtc.Minute;
            SelectedTime = new TimeSpan(SelectedHour, SelectedMinute, 0);

            DaysOfWeek = alarm.DaysOfWeek;
            IsEnabled = alarm.IsEnabled;
            Notes = alarm.Notes;
            
            // Load new fields
            if (Enum.TryParse<RepeatType>(alarm.RepeatType, out var repeatType))
            {
                SelectedRepeatType = repeatType;
            }
            IsRepeating = alarm.IsRepeating;
            SelectedDate = alarm.SelectedDate?.ToLocalTime(); // Convert from UTC to local time
            ChurchId = alarm.ChurchId;

            // Parse the existing alarm to determine repeat type and set day flags
            ParseAlarmRepeatType(alarm.DaysOfWeek);
        }


        private async Task SaveAsync()
        {
            System.Diagnostics.Debug.WriteLine($"=== SaveAsync START ===");
            System.Diagnostics.Debug.WriteLine($"SelectedRepeatType: {SelectedRepeatType}");
            System.Diagnostics.Debug.WriteLine($"IsRepeating: {IsRepeating}");
            System.Diagnostics.Debug.WriteLine($"SelectedDate: {SelectedDate}");
            System.Diagnostics.Debug.WriteLine($"ChurchId: {ChurchId}");
            
            var time = new TimeOnly(SelectedHour, SelectedMinute);
            var daysOfWeek = ComputeDaysOfWeekFromNewSystem();
            
            var model = new Alarm
            {
                Id = Id,
                BellName = BellName,
                HourUtc = time,
                DaysOfWeek = daysOfWeek,
                IsEnabled = IsEnabled,
                Notes = Notes,
                RepeatType = SelectedRepeatType.ToString(),
                IsRepeating = IsRepeating,
                SelectedDate = SelectedDate?.ToUniversalTime(), // Convert to UTC for PostgreSQL
                ChurchId = ChurchId
            };

            bool ok;
            if (Id == 0)
            {
                System.Diagnostics.Debug.WriteLine($"Creating new alarm...");
                var created = await _api.CreateAsync(model);
                ok = created != null;
                System.Diagnostics.Debug.WriteLine($"Create result: {ok}, Created alarm: {created?.Id}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Updating existing alarm {Id}...");
                ok = await _api.UpdateAsync(model);
                System.Diagnostics.Debug.WriteLine($"Update result: {ok}");
            }

            if (ok)
            {
                System.Diagnostics.Debug.WriteLine($"Save successful, navigating back...");
                await Shell.Current.GoToAsync("..", true);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Save failed!");
            }
            System.Diagnostics.Debug.WriteLine($"=== SaveAsync END ===");
        }


        private List<string> ComputeDaysOfWeekFromNewSystem()
        {
            switch (SelectedRepeatType)
            {
                case RepeatType.Once:
                    if (!IsRepeating)
                    {
                        // One-time: choose next occurrence today or tomorrow depending on time
                        var nowUtc = DateTime.UtcNow;
                        var todayTime = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, SelectedHour, SelectedMinute, 0, DateTimeKind.Utc);
                        var next = todayTime > nowUtc ? todayTime : todayTime.AddDays(1);
                        var day = next.DayOfWeek; // Sunday..Saturday
                        return new List<string> { DayOfWeekToShort(day) };
                    }
                    else
                    {
                        // Once with weekly repeat - repeat on the same day every week
                        var nowUtc = DateTime.UtcNow;
                        var todayTime = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, SelectedHour, SelectedMinute, 0, DateTimeKind.Utc);
                        var next = todayTime > nowUtc ? todayTime : todayTime.AddDays(1);
                        var day = next.DayOfWeek;
                        return new List<string> { DayOfWeekToShort(day) };
                    }
                    
                case RepeatType.Custom:
                    // Check if a specific date is selected
                    if (SelectedDate.HasValue)
                    {
                        // For specific dates, we need to check if the date is in the future
                        var nowUtc = DateTime.UtcNow;
                        var selectedDateTime = new DateTime(SelectedDate.Value.Year, SelectedDate.Value.Month, SelectedDate.Value.Day, SelectedHour, SelectedMinute, 0, DateTimeKind.Utc);
                        
                        if (selectedDateTime > nowUtc)
                        {
                            // Use the specific date
                            return new List<string> { DayOfWeekToShort(SelectedDate.Value.DayOfWeek) };
                        }
                        else
                        {
                            // Date is in the past, use next occurrence of that day
                            var nextOccurrence = selectedDateTime;
                            while (nextOccurrence <= nowUtc)
                            {
                                nextOccurrence = nextOccurrence.AddDays(7);
                            }
                            return new List<string> { DayOfWeekToShort(nextOccurrence.DayOfWeek) };
                        }
                    }
                    else
                    {
                        // Custom days (whether repeating or once)
                        var list = new List<string>();
                        if (RepeatSun) list.Add("Sun");
                        if (RepeatMon) list.Add("Mon");
                        if (RepeatTue) list.Add("Tue");
                        if (RepeatWed) list.Add("Wed");
                        if (RepeatThu) list.Add("Thu");
                        if (RepeatFri) list.Add("Fri");
                        if (RepeatSat) list.Add("Sat");
                        
                        // If no days selected but it's Custom, maybe default to today? 
                        // Or just return empty list (which might be invalid).
                        // Let's return empty list for now, validation should handle it if needed.
                        return list;
                    }
                    
                default:
                    return new List<string>();
            }
        }

        private static string DayOfWeekToShort(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Sunday => "Sun",
                DayOfWeek.Monday => "Mon",
                DayOfWeek.Tuesday => "Tue",
                DayOfWeek.Wednesday => "Wed",
                DayOfWeek.Thursday => "Thu",
                DayOfWeek.Friday => "Fri",
                DayOfWeek.Saturday => "Sat",
                _ => ""
            };
        }

        private void SetWeeklyFlags(IEnumerable<string> days)
        {
            var set = new HashSet<string>(days, StringComparer.OrdinalIgnoreCase);
            RepeatSun = set.Contains("Sun");
            RepeatMon = set.Contains("Mon");
            RepeatTue = set.Contains("Tue");
            RepeatWed = set.Contains("Wed");
            RepeatThu = set.Contains("Thu");
            RepeatFri = set.Contains("Fri");
            RepeatSat = set.Contains("Sat");
        }

        private void ParseAlarmRepeatType(List<string> daysOfWeek)
        {
            if (daysOfWeek == null || !daysOfWeek.Any()) 
            {
                // Only set properties if not already loaded from alarm data
                if (SelectedRepeatType == RepeatType.Once && !IsRepeating && SelectedDate == null)
                {
                    // This is likely a new alarm, set defaults
                    return; // Already set in LoadFrom
                }
                return;
            }
            
            var days = daysOfWeek.Select(d => d.Trim()).ToList();
            
            // Only set day flags, don't change SelectedRepeatType as it's already loaded from alarm data
            if (days.Count == 1 && days.Contains("Sun"))
            {
                RepeatSun = true;
                RepeatMon = false;
                RepeatTue = false;
                RepeatWed = false;
                RepeatThu = false;
                RepeatFri = false;
                RepeatSat = false;
            }
            else if (days.Count > 1)
            {
                SetWeeklyFlags(days);
            }
            else
            {
                SetWeeklyFlags(days);
            }
        }

        private async Task LoadChurchInfoAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== LoadChurchInfoAsync START ===");
                var user = await _userService.GetCurrentUserAsync();
                System.Diagnostics.Debug.WriteLine($"User: {user?.Username}, ChurchId: {user?.ChurchId}, Role: {user?.Role}");
                
                if (user?.ChurchId != null)
                {
                    System.Diagnostics.Debug.WriteLine($"User has ChurchId: {user.ChurchId}");
                    // Get church information from the new user-specific endpoint
                    try
                    {
                        var httpClient = ServiceHelper.GetRequiredService<HttpClient>();
                        var response = await httpClient.GetAsync("api/auth/my-church");
                        System.Diagnostics.Debug.WriteLine($"My church API response status: {response.StatusCode}");
                        
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonContent = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"My church API returned JSON: {jsonContent}");
                            
                            if (!string.IsNullOrEmpty(jsonContent) && jsonContent != "null")
                            {
                                var churchData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                                ChurchName = churchData.GetProperty("name").GetString() ?? "Unknown Church";
                                ChurchPhone = ""; // Hide phone number from users
                                ChurchId = churchData.GetProperty("id").GetInt32();
                                HasChurch = true;
                                System.Diagnostics.Debug.WriteLine($"Set ChurchName: {ChurchName}, ChurchId: {ChurchId}");
                            }
                            else
                            {
                                ChurchName = "No church assigned";
                                ChurchPhone = ""; // Hide phone number from users
                                ChurchId = null;
                                HasChurch = false;
                                System.Diagnostics.Debug.WriteLine("My church API returned null or empty");
                            }
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine($"My church API error: {errorContent}");
                            ChurchName = "Unable to load church info";
                            ChurchPhone = ""; // Hide phone number from users
                            ChurchId = null;
                            HasChurch = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Exception calling my-church API: {ex.Message}");
                        ChurchName = "Unable to load church info";
                        ChurchPhone = ""; // Hide phone number from users
                        ChurchId = null;
                        HasChurch = false;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("User has no ChurchId");
                    ChurchName = "No church assigned";
                    ChurchPhone = ""; // Hide phone number from users
                    ChurchId = null;
                    HasChurch = false;
                }
                System.Diagnostics.Debug.WriteLine("=== LoadChurchInfoAsync END ===");
            }
            catch (Exception ex)
            {
                // If church info can't be loaded, show a message
                System.Diagnostics.Debug.WriteLine($"Error loading church info: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ChurchName = "Unable to load church info";
                ChurchPhone = ""; // Hide phone number from users
                ChurchId = null;
                HasChurch = false;
            }
        }

        public async Task RefreshChurchInfoAsync()
        {
            await LoadChurchInfoAsync();
        }

        private async Task OpenDatePickerAsync()
        {
            try
            {
                // This method is no longer needed since we use direct click events
                System.Diagnostics.Debug.WriteLine("OpenDatePickerAsync called - handled by direct click event");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening date picker: {ex.Message}");
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
	}
}


