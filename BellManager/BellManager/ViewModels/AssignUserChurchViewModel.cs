using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BellManager.Models;
using BellManager.Services;

namespace BellManager.ViewModels
{
    public class AssignUserChurchViewModel : INotifyPropertyChanged
    {
        private readonly AdminUserApiService _adminUsers;
        private readonly ChurchApiService _churches;

        public ObservableCollection<AdminUserApiService.UserLite> Users { get; } = new();
        public ObservableCollection<Church> Churches { get; } = new();
        public ObservableCollection<UserAssignmentDisplay> UserAssignments { get; } = new();

        public ICommand LoadCommand { get; }
        public ICommand AssignCommand { get; }
        public ICommand RefreshAssignmentsCommand { get; }
        public ICommand EditAssignmentCommand { get; }
        public ICommand SaveEditAssignmentCommand { get; }
        public ICommand CancelEditAssignmentCommand { get; }
        public ICommand DeleteAssignmentCommand { get; }

        // Edit popup properties
        private UserAssignmentDisplay? _editingAssignment;
        public UserAssignmentDisplay? EditingAssignment 
        { 
            get => _editingAssignment; 
            set => SetProperty(ref _editingAssignment, value); 
        }

        public AssignUserChurchViewModel(AdminUserApiService adminUsers, ChurchApiService churches)
        {
            _adminUsers = adminUsers;
            _churches = churches;
            LoadCommand = new Command(async () => await LoadAsync());
            AssignCommand = new Command<object>(async (param) => await AssignAsync(param));
            RefreshAssignmentsCommand = new Command(async () => await LoadAssignmentsAsync());
            EditAssignmentCommand = new Command<UserAssignmentDisplay>(async (assignment) => await EditAssignmentAsync(assignment));
            SaveEditAssignmentCommand = new Command(async () => await SaveEditAssignmentAsync());
            CancelEditAssignmentCommand = new Command(async () => await CancelEditAssignmentAsync());
            DeleteAssignmentCommand = new Command<UserAssignmentDisplay>(async (assignment) => await DeleteAssignmentAsync(assignment));
        }

        public async Task LoadAsync()
        {
            await LoadUsersAsync();
            await LoadChurchesAsync();
            await LoadAssignmentsAsync();
        }

        private async Task LoadUsersAsync()
        {
            Users.Clear();
            var users = await _adminUsers.GetUsersAsync();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }

        private async Task LoadChurchesAsync()
        {
            Churches.Clear();
            var churches = await _churches.GetAllAsync();
            foreach (var church in churches)
            {
                Churches.Add(church);
            }
        }

        public async Task LoadAssignmentsAsync()
        {
            UserAssignments.Clear();
            var users = await _adminUsers.GetUsersAsync();
            var churches = await _churches.GetAllAsync();

            System.Diagnostics.Debug.WriteLine($"Loading assignments: {users.Count} users, {churches.Count} churches");

            foreach (var user in users)
            {
                System.Diagnostics.Debug.WriteLine($"User: {user.UserName}, ChurchId: {user.ChurchId}");
                if (user.ChurchId.HasValue)
                {
                    var church = churches.FirstOrDefault(c => c.Id == user.ChurchId.Value);
                    System.Diagnostics.Debug.WriteLine($"Found church: {church?.Name}");
                    if (church != null)
                    {
                        UserAssignments.Add(new UserAssignmentDisplay
                        {
                            UserId = user.Id,
                            UserName = user.UserName,
                            UserEmail = user.Email,
                            ChurchId = church.Id,
                            ChurchName = church.Name,
                            ChurchPhone = church.PhoneNumber
                        });
                        System.Diagnostics.Debug.WriteLine($"Added assignment: {user.UserName} -> {church.Name}");
                    }
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"Total assignments loaded: {UserAssignments.Count}");
        }

        private async Task AssignAsync(object parameter)
        {
            if (parameter is not Tuple<AdminUserApiService.UserLite, Church> assignment)
                return;

            var (user, church) = assignment;
            
            // Clear any previous error
            HasError = false;
            ErrorMessage = string.Empty;
            
            try
            {
                // Check if user already has a church assignment
                var existingAssignment = UserAssignments.FirstOrDefault(a => a.UserId == user.Id);
                if (existingAssignment != null)
                {
                    // User already has a church - show error message
                    ErrorMessage = $"User '{user.UserName}' is already assigned to a church. Use the Edit button to change the assignment.";
                    HasError = true;
                    return;
                }

                // New assignment
                var ok = await _adminUsers.AssignChurchAsync(user.Id, church.Id);
                if (ok)
                {
                    await LoadAssignmentsAsync();
                    // Clear any previous error on success
                    HasError = false;
                    ErrorMessage = string.Empty;
                }
                else
                {
                    ErrorMessage = "Failed to assign user to church. Please try again.";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error assigning user to church: {ex.Message}");
                ErrorMessage = "An error occurred while assigning user to church. Please try again.";
                HasError = true;
            }
        }

        private async Task EditAssignmentAsync(UserAssignmentDisplay assignment)
        {
            EditingAssignment = assignment;
            OnPropertyChanged(nameof(EditingAssignment));
        }

        private async Task SaveEditAssignmentAsync()
        {
            if (EditingAssignment == null) return;

            try
            {
                // This will be handled by the code-behind to get the picker value
                // For now, we'll need to pass the selected church through a property
                if (SelectedEditChurch == null) return;

                var ok = await _adminUsers.AssignChurchAsync(EditingAssignment.UserId, SelectedEditChurch.Id);
                if (ok)
                {
                    EditingAssignment = null;
                    OnPropertyChanged(nameof(EditingAssignment));
                    await LoadAssignmentsAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating assignment: {ex.Message}");
            }
        }

        // Property to hold the selected church for editing
        private Church? _selectedEditChurch;
        public Church? SelectedEditChurch 
        { 
            get => _selectedEditChurch; 
            set => SetProperty(ref _selectedEditChurch, value); 
        }

        // Error message properties
        private string _errorMessage = string.Empty;
        public string ErrorMessage 
        { 
            get => _errorMessage; 
            set => SetProperty(ref _errorMessage, value); 
        }

        private bool _hasError = false;
        public bool HasError 
        { 
            get => _hasError; 
            set => SetProperty(ref _hasError, value); 
        }

        private async Task CancelEditAssignmentAsync()
        {
            EditingAssignment = null;
            OnPropertyChanged(nameof(EditingAssignment));
        }

        private async Task DeleteAssignmentAsync(UserAssignmentDisplay assignment)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Attempting to delete assignment for user {assignment.UserName} (ID: {assignment.UserId})");
                // Remove church assignment
                var ok = await _adminUsers.RemoveChurchAssignmentAsync(assignment.UserId);
                System.Diagnostics.Debug.WriteLine($"Delete result: {ok}");
                if (ok)
                {
                    await LoadAssignmentsAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting assignment: {ex.Message}");
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

    public class UserAssignmentDisplay : INotifyPropertyChanged
    {
        private int _userId;
        private string _userName = string.Empty;
        private string _userEmail = string.Empty;
        private int _churchId;
        private string _churchName = string.Empty;
        private string _churchPhone = string.Empty;

        public int UserId 
        { 
            get => _userId; 
            set => SetProperty(ref _userId, value); 
        }
        public string UserName 
        { 
            get => _userName; 
            set => SetProperty(ref _userName, value); 
        }
        public string UserEmail 
        { 
            get => _userEmail; 
            set => SetProperty(ref _userEmail, value); 
        }
        public int ChurchId 
        { 
            get => _churchId; 
            set => SetProperty(ref _churchId, value); 
        }
        public string ChurchName 
        { 
            get => _churchName; 
            set => SetProperty(ref _churchName, value); 
        }
        public string ChurchPhone 
        { 
            get => _churchPhone; 
            set => SetProperty(ref _churchPhone, value); 
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
