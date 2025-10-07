using BellManager.Helpers;
using BellManager.ViewModels;
using BellManager.Models;
using BellManager.Services;

namespace BellManager.Pages;

public partial class AssignUserChurchPage : ContentPage
{
	private readonly AssignUserChurchViewModel _viewModel;

	public AssignUserChurchPage()
	{
		InitializeComponent();
		_viewModel = ServiceHelper.GetRequiredService<AssignUserChurchViewModel>();
		BindingContext = _viewModel;
		
		// Subscribe to property changes to show/hide popup
		_viewModel.PropertyChanged += OnViewModelPropertyChanged;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadAsync();
	}

	public async Task InitializeAsync()
	{
		await _viewModel.LoadAsync();
	}

	public void TriggerOnAppearing()
	{
		OnAppearing();
	}

	private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(AssignUserChurchViewModel.EditingAssignment))
		{
			if (_viewModel.EditingAssignment != null)
			{
				// Show popup and populate fields
				EditAssignmentPopup.IsVisible = true;
				EditUserLabel.Text = $"Editing assignment for: {_viewModel.EditingAssignment.UserName}";
				
				// Set the current church as selected
				var currentChurch = _viewModel.Churches.FirstOrDefault(c => c.Id == _viewModel.EditingAssignment.ChurchId);
				EditChurchPicker.SelectedItem = currentChurch;
				_viewModel.SelectedEditChurch = currentChurch;
			}
			else
			{
				// Hide popup
				EditAssignmentPopup.IsVisible = false;
			}
		}
	}

	private void OnEditChurchPickerSelectionChanged(object sender, EventArgs e)
	{
		if (EditChurchPicker.SelectedItem is Church selectedChurch)
		{
			_viewModel.SelectedEditChurch = selectedChurch;
		}
	}

	private async void OnAssignClicked(object sender, EventArgs e)
	{
		if (UserPicker.SelectedItem is not AdminUserApiService.UserLite user || ChurchPicker.SelectedItem is not Church church)
		{
			_viewModel.ErrorMessage = "Please select both a user and a church.";
			_viewModel.HasError = true;
			return;
		}
		
		// Execute the assignment command
		_viewModel.AssignCommand.Execute(Tuple.Create(user, church));
		
		// Refresh the assignments list
		await _viewModel.LoadAssignmentsAsync();
		
		// Clear selections on success
		if (!_viewModel.HasError)
		{
			UserPicker.SelectedItem = null;
			ChurchPicker.SelectedItem = null;
		}
	}
}


