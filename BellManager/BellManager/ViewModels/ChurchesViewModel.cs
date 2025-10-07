using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BellManager.Models;
using BellManager.Services;

namespace BellManager.ViewModels
{
	public class ChurchesViewModel : INotifyPropertyChanged
	{
		private readonly ChurchApiService _churchService;
		public ObservableCollection<ChurchDisplayModel> Churches { get; } = new();
		public string? Name { get; set; }
		public string? Phone { get; set; }

		// Edit popup properties
		private ChurchDisplayModel? _editingChurch;
		public ChurchDisplayModel? EditingChurch 
		{ 
			get => _editingChurch; 
			set => SetProperty(ref _editingChurch, value); 
		}

		public ICommand AddCommand { get; }
		public ICommand EditCommand { get; }
		public ICommand SaveEditCommand { get; }
		public ICommand CancelEditCommand { get; }
		public ICommand DeleteCommand { get; }

		public ChurchesViewModel(ChurchApiService churchService)
		{
			_churchService = churchService;
			AddCommand = new Command(async () => await AddAsync());
			EditCommand = new Command<ChurchDisplayModel>(async (church) => await EditAsync(church));
			SaveEditCommand = new Command(async () => await SaveEditAsync());
			CancelEditCommand = new Command(async () => await CancelEditAsync());
			DeleteCommand = new Command<ChurchDisplayModel>(async (church) => await DeleteAsync(church));
		}

	public async Task LoadAsync()
	{
		try
		{
			Churches.Clear();
			var churches = await _churchService.GetAllAsync();
			System.Diagnostics.Debug.WriteLine($"Loaded {churches.Count()} churches from API");
			
			foreach (var c in churches) 
			{
				Churches.Add(new ChurchDisplayModel(c));
				System.Diagnostics.Debug.WriteLine($"Added church: {c.Name}");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error loading churches: {ex.Message}");
		}
	}

		public async Task<bool> AddAsync()
		{
			if (string.IsNullOrWhiteSpace(Name)) return false;
			var created = await _churchService.CreateAsync(new Church { Name = Name!, PhoneNumber = Phone ?? string.Empty });
			if (created is null) return false;
			Churches.Add(new ChurchDisplayModel(created));
			Name = Phone = string.Empty;
			OnPropertyChanged(nameof(Name));
			OnPropertyChanged(nameof(Phone));
			return true;
		}

		public async Task<bool> EditAsync(ChurchDisplayModel church)
		{
			EditingChurch = church;
			// Trigger popup visibility change
			OnPropertyChanged(nameof(EditingChurch));
			return true;
		}

		public async Task<bool> SaveEditAsync()
		{
			if (EditingChurch == null) return false;
			
			// Update the church with new values from popup
			EditingChurch.Name = _editingName ?? EditingChurch.Name;
			EditingChurch.PhoneNumber = _editingPhone ?? EditingChurch.PhoneNumber;
			
			var updated = await _churchService.UpdateAsync(new Church 
			{ 
				Id = EditingChurch.Id, 
				Name = EditingChurch.Name, 
				PhoneNumber = EditingChurch.PhoneNumber 
			});
			
			if (updated)
			{
				EditingChurch = null;
				OnPropertyChanged(nameof(EditingChurch));
			}
			return updated;
		}

		// Properties to hold popup values
		private string? _editingName;
		private string? _editingPhone;
		
		public string? EditingName 
		{ 
			get => _editingName; 
			set => SetProperty(ref _editingName, value); 
		}
		
		public string? EditingPhone 
		{ 
			get => _editingPhone; 
			set => SetProperty(ref _editingPhone, value); 
		}

		public async Task<bool> CancelEditAsync()
		{
			EditingChurch = null;
			OnPropertyChanged(nameof(EditingChurch));
			return true;
		}

		public async Task<bool> DeleteAsync(ChurchDisplayModel church)
		{
			var ok = await _churchService.DeleteAsync(church.Id);
			if (ok) Churches.Remove(church);
			return ok;
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string? name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = value;
			OnPropertyChanged(propertyName);
			return true;
		}
	}

	public class ChurchDisplayModel : INotifyPropertyChanged
	{
		private string _name;
		private string _phoneNumber;

		public int Id { get; set; }
		public string Name 
		{ 
			get => _name; 
			set 
			{ 
				_name = value; 
				OnPropertyChanged(); 
			} 
		}
		public string PhoneNumber 
		{ 
			get => _phoneNumber; 
			set 
			{ 
				_phoneNumber = value; 
				OnPropertyChanged(); 
			} 
		}

		public ChurchDisplayModel(Church church)
		{
			Id = church.Id;
			_name = church.Name;
			_phoneNumber = church.PhoneNumber;
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}


