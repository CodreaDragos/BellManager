namespace BellManager
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            base.OnStart();
            try
            {
                var auth = Helpers.ServiceHelper.Services.GetService(typeof(Services.AuthApiService)) as Services.AuthApiService;
                if (auth is not null)
                {
                    var hasToken = await auth.TryAttachSavedTokenAsync();
                    if (!hasToken)
                    {
                        await Shell.Current.GoToAsync("/Login");
                    }
                    else
                    {
                        // Navigate to Alarms page if user is authenticated
                        await Shell.Current.GoToAsync("//Alarms");
                    }
                }
            }
            catch { }
        }
    }
}
