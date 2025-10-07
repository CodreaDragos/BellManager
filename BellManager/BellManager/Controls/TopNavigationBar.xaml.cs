using Microsoft.Maui.Controls;

namespace BellManager.Controls
{
    public partial class TopNavigationBar : ContentView
    {
        public event EventHandler<string> TabSelected;
        public event EventHandler LogoutClicked;

        public TopNavigationBar()
        {
            InitializeComponent();
        }

        public void SetActiveTab(string tabName)
        {
            // Reset all buttons to inactive style
            AlarmsButton.Style = (Style)Resources["NavButtonStyle"];
            ChurchesButton.Style = (Style)Resources["NavButtonStyle"];
            AssignButton.Style = (Style)Resources["NavButtonStyle"];

            // Set the active tab
            switch (tabName.ToLower())
            {
                case "alarms":
                    AlarmsButton.Style = (Style)Resources["ActiveNavButtonStyle"];
                    break;
                case "churches":
                    ChurchesButton.Style = (Style)Resources["ActiveNavButtonStyle"];
                    break;
                case "assign":
                    AssignButton.Style = (Style)Resources["ActiveNavButtonStyle"];
                    break;
            }
        }

        public void SetAdminTabsVisibility(bool isAdmin)
        {
            ChurchesButton.IsVisible = isAdmin;
            AssignButton.IsVisible = isAdmin;
        }

        private void OnAlarmsClicked(object sender, EventArgs e)
        {
            SetActiveTab("alarms");
            TabSelected?.Invoke(this, "Alarms");
        }

        private void OnChurchesClicked(object sender, EventArgs e)
        {
            SetActiveTab("churches");
            TabSelected?.Invoke(this, "Churches");
        }

        private void OnAssignClicked(object sender, EventArgs e)
        {
            SetActiveTab("assign");
            TabSelected?.Invoke(this, "Assign");
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            LogoutClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
