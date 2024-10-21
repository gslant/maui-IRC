using IRC.Models;
using IRC.Services;
using IRC.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using static IRC.Models.Message;

namespace IRC
{
    public partial class ConnectionPage : ContentPage
    {
        private readonly ConnectionViewModel _connectionViewModel;
        public ConnectionPage(string hostname, int port, string nickname, string username, string realname, string? password,
                          Func<string, int, string, string, string, string?, ConnectionViewModel> viewModelFactory)
        {
            InitializeComponent();
            // Set the BindingContext to an instance of ConnectionViewModel
            _connectionViewModel = viewModelFactory(hostname, port, nickname, username, realname, password);
            BindingContext = _connectionViewModel;

            InitializeConnection(hostname, port);

            _connectionViewModel.MessageAdded += OnMessageAdded;
        }

        private async void InitializeConnection(string hostname, int port)
        {
            try
            {
                // Call a method in the ViewModel to initialize the connection (TCPClient)
                var viewModel = (ConnectionViewModel)BindingContext;
                await viewModel.InitializeTcpClient();

                // Connection successful, continue as normal
            }
            catch (Exception ex)
            {
                // Connection failed, navigate back to the form and display an error message
                await DisplayAlert("Connection Error", $"Failed to connect: {ex.Message}", "OK");

                // Navigate back to the previous page
                await Navigation.PopAsync();
            }
        }

        // If the message is sent by the user, scroll the page to the bottom
        private void OnMessageAdded()
        {
            var currentChannel = ((ConnectionViewModel)BindingContext).CurrentChannel;

            if (currentChannel.Messages.Count > 0)
            {
                // Add a delay between scrolls to simulate smooth scrolling
                this.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(100), () =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            if(MessageStackLayout.Children.Count > 0)
                            {
                                var lastLabel = (Element)MessageStackLayout.Children[^1];
                                await MessageScrollView.ScrollToAsync(lastLabel, ScrollToPosition.End, true);
                            }

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    });

                    return false; // Run only once
                });
            }
        }

        // Event handler for the Loaded event
        private void OnPageLoaded(object sender, EventArgs e)
        {

        }

        // Event handler for the Unloaded event
       private async void OnPageUnloaded(object sender, EventArgs e)
        {
            if (BindingContext is ConnectionViewModel viewModel)
            {
                await viewModel.Cleanup();
            }
        }

        private void MessageEntry_Completed(object sender, EventArgs e)
        {
            var viewModel = (ConnectionViewModel)BindingContext;
            if (viewModel.SendCommand.CanExecute(MessageEntry.Text))
            {
                viewModel.SendCommand.Execute(MessageEntry.Text);
            }
        }
    }
}
