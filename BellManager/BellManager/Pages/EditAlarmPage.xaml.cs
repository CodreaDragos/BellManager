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
            
            // Always refresh church information when page appears
            await _viewModel.RefreshChurchInfoAsync();
            
            // Note: Alarm loading is now handled by the ViewModel's QueryProperty "AlarmId"
            // We don't need to manually parse the query string here anymore.
        }



        private void OnQuickTimeClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string timeString)
            {
                if (TimeOnly.TryParse(timeString, out var time))
                {
                    _viewModel.SelectedTime = time.ToTimeSpan();
                }
            }
        }
    }
}
