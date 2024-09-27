using IRC.Models;
using IRC.Services;
using IRC.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;

namespace IRC
{
    public partial class ConnectionPage : ContentPage
    {
        public ConnectionPage(string hostname, int port, string nickname, string username, string realname, string? password)
        {
            InitializeComponent();
            // Set the BindingContext to an instance of ConnectionViewModel
            var viewModel = new ConnectionViewModel(hostname, port, nickname, username, realname, password);
            BindingContext = viewModel;

            InitializeConnection(hostname, port);

            viewModel.MessageAdded += OnMessageAdded;
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

        private void OnMessageAdded(bool isUserMessage)
        {
            if (isUserMessage && ((ConnectionViewModel)BindingContext).CurrentChannel.Messages.Count > 0)
            {
                // Add a delay between scrolls to simulate smooth scrolling
                this.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(100), () =>
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        var lastLabel = (Element)MessageStackLayout.Children[^1];
                        await MessageScrollView.ScrollToAsync(lastLabel, ScrollToPosition.End, true);
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
        private void OnPageUnloaded(object sender, EventArgs e)
        {
            if (BindingContext is ConnectionViewModel viewModel)
            {
                viewModel.Cleanup();
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

public class Channel : INotifyPropertyChanged
{
    public string Name { get; set; }
    private ObservableCollection<MessageDisplay> _messages;
    public ObservableCollection<MessageDisplay> Messages
    {
        get => _messages;
        set
        {
            _messages = value;
            OnPropertyChanged(nameof(Messages));
        }
    }

    public Channel(string name)
    {
        Name = name;
        Messages = new ObservableCollection<MessageDisplay>();
    }

    public void AddMessage(string text, MessageType type)
    {
        Messages.Add(new MessageDisplay(text, type));
        OnPropertyChanged(nameof(Messages));
    }

    public static Channel? GetChannelByName(ObservableCollection<Channel> channels, string name)
    {
        return channels.FirstOrDefault(c => c.Name == name);
    }

    public static bool ContainsChannelByName(ObservableCollection<Channel> channels, string name)
    {
        return channels.Any(c => c.Name == name);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}