using BellManager.Helpers;
using BellManager.ViewModels;

namespace BellManager.Pages;

public partial class ChurchesPage : ContentPage
{
	private readonly ChurchesViewModel _vm;

	public ChurchesPage()
	{
		InitializeComponent();
		_vm = ServiceHelper.GetRequiredService<ChurchesViewModel>();
		BindingContext = _vm;
		
		// Subscribe to property changes to show/hide popup
		_vm.PropertyChanged += OnViewModelPropertyChanged;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _vm.LoadAsync();
	}

	public async Task InitializeAsync()
	{
		await _vm.LoadAsync();
	}

	public void TriggerOnAppearing()
	{
		OnAppearing();
	}

	private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ChurchesViewModel.EditingChurch))
		{
			if (_vm.EditingChurch != null)
			{
				// Show popup and populate fields
				EditPopup.IsVisible = true;
				_vm.EditingName = _vm.EditingChurch.Name;
				_vm.EditingPhone = _vm.EditingChurch.PhoneNumber;
			}
			else
			{
				// Hide popup
				EditPopup.IsVisible = false;
			}
		}
	}

	private async void OnAddClicked(object sender, EventArgs e)
	{
		ErrorLabel.IsVisible = false;
		if (!await _vm.AddAsync())
		{
			ErrorLabel.Text = "Failed to add church";
			ErrorLabel.IsVisible = true;
		}
	}

	private async void OnSaveClicked(object sender, EventArgs e)
	{
		ErrorLabel.IsVisible = false;
		if (!await _vm.SaveEditAsync())
		{
			ErrorLabel.Text = "Failed to save church";
			ErrorLabel.IsVisible = true;
		}
	}

	private async void OnDeleteClicked(object sender, EventArgs e)
	{
		ErrorLabel.IsVisible = false;
		var btn = (Button)sender;
		var church = (ChurchDisplayModel)btn.BindingContext;
		if (!await _vm.DeleteAsync(church))
		{
			ErrorLabel.Text = "Failed to delete church";
			ErrorLabel.IsVisible = true;
		}
	}
}


