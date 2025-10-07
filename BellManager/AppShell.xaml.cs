using BellManager.Services;
using BellManager.Helpers;

namespace BellManager
{
    public partial class AppShell : Shell
    {
        private readonly UserService _userService;

        public AppShell()
        {
            InitializeComponent();
            _userService = ServiceHelper.GetRequiredService<UserService>();
            
            Routing.RegisterRoute("Login", typeof(Pages.LoginPage));
            Routing.RegisterRoute("Register", typeof(Pages.RegisterPage));
            Routing.RegisterRoute("EditAlarm", typeof(Pages.EditAlarmPage));
            Routing.RegisterRoute("Churches", typeof(Pages.ChurchesPage));
            Routing.RegisterRoute("AssignUserChurch", typeof(Pages.AssignUserChurchPage));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await UpdateAdminTabsVisibility();
        }

        public async Task RefreshAdminTabsAsync()
        {
            await UpdateAdminTabsVisibility();
        }

        private async Task UpdateAdminTabsVisibility()
        {
            var isAdmin = await _userService.IsAdminAsync();
            ChurchesTab.IsVisible = isAdmin;
            AssignTab.IsVisible = isAdmin;
        }
    }
}
