using System.ComponentModel;
using System.Net.Sockets;

namespace IRC
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            // Validation: Check if mandatory fields are filled
            if (string.IsNullOrWhiteSpace(NicknameEntry.Text) ||
                string.IsNullOrWhiteSpace(UsernameEntry.Text) ||
                string.IsNullOrWhiteSpace(RealnameEntry.Text) ||
                string.IsNullOrWhiteSpace(HostnameEntry.Text) ||
                string.IsNullOrWhiteSpace(PortEntry.Text))
            {
                await DisplayAlert("Error", "Please fill out all mandatory fields.", "OK");
                return;
            }

            string nickname = NicknameEntry.Text;
            string username = UsernameEntry.Text;
            string realname = RealnameEntry.Text;
            string hostname = HostnameEntry.Text;
            int port = int.Parse(PortEntry.Text);
            string password = PasswordEntry.Text; // Optional field

            try
            {  
               await Navigation.PushAsync(new ConnectionPage(hostname, port, nickname, username, realname, password));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Navigation Error", $"Failed to navigate to ConnectionPage: {ex.Message}", "OK");
            }
        }

    }
}
