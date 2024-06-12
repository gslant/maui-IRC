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
        public ConnectionPage(string hostname, int port)
        {
            InitializeComponent();

            // Set the BindingContext to an instance of ConnectionViewModel
            var viewModel = new ConnectionViewModel(hostname, port);
            BindingContext = viewModel;

            viewModel.MessageAdded += (isUserMessage) =>
            {
                if (isUserMessage && viewModel.CurrentChannel.Messages.Count > 0)
                {
                    // Add a delay between scrolls to simulate smooth scrolling
                    Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            var lastLabel = (Element)MessageStackLayout.Children[^1];
                            await MessageScrollView.ScrollToAsync(lastLabel, ScrollToPosition.End, true);
                        });

                        return false; // Run only once
                    });
                }
            };
        }

        // Event handler for the Loaded event
        private void OnPageLoaded(object sender, EventArgs e)
        {
            // Any additional logic you might want to execute when the page is loaded
        }

        // Event handler for the Unloaded event
        private void OnPageUnloaded(object sender, EventArgs e)
        {
            if (BindingContext is ConnectionViewModel viewModel)
            {
                viewModel.Cleanup();
            }
        }
    }
}
/*
public partial class ConnectionPage : ContentPage
{
    TcpClient tcpClient;
    StreamReader reader;
    StreamWriter writer;

    //TODO these need to be filled out by the user before connection. Possibly add some defaults
    static string nick;
    static string username;
    static string realName;

    //context for the current channel or user
    string channelCtx;

    private ObservableCollection<Channel> channels;
    Channel currentChannel;
    //Type used to color message responses in an earlier iteration. May be depreciated
    enum MessageType
    {
        NOTICE,
        GENERIC_SERVERMSG,
        DEBUG_MSG,
        GENERIC_RESPONSE
    }

    public ConnectionPage(string hostname, int port)
    {
        InitializeComponent();
        //TODO allow selection of server/service and port by user. Specs define a range of commonly allowed ports?
        tcpClient = new TcpClient(hostname, port);
        reader = new StreamReader(tcpClient.GetStream());
        writer = new StreamWriter(tcpClient.GetStream());

        channels = new ObservableCollection<Channel>();
        Channel defaultChan = new Channel("default");
        channels.Add(defaultChan);
        //Defaults
        nick = "billbob123";
        username = "billbob123";
        realName = "billbob123";

        var defaultMessageStack = new StackLayout
        {
            VerticalOptions = LayoutOptions.StartAndExpand
        };
        MessageScrollView.Content = defaultMessageStack;
        defaultChan.messageStack = defaultMessageStack;
        currentChannel = defaultChan;

        //var channelManager = new ChannelManager();
        //channelManager.CurrentChannelChanged += (sender, args) =>
        //{
        //    channelPicker.SelectedItem = ((ChannelManager)sender).CurrentChannel;
        //};

        //channelPicker.ItemsSource = channels;
        //channelPicker.ItemDisplayBinding = new Binding("Name");
        //channelPicker.SelectedItem = new Binding(currentChannel.Name, BindingMode.TwoWay);

    }

    //Ran as soon as this page is loaded
    private async void OnPageLoaded(object sender, EventArgs e)
    {
        //Mainly to handle IO or socket exceptions, also good for any async funkiness
        try
        {

            Message nickMsg = new Message();
            nickMsg.Command = "NICK";
            nickMsg.Args = new List<string> { nick };
            WriteMessageCommand(nickMsg);

            Message userMsg = new Message();
            userMsg.Command = "USER";
            userMsg.Args = new List<string>() {username, "0 * :", realName };
            WriteMessageCommand(userMsg);


            while (true)
            {

                string? serverMsg = await reader.ReadLineAsync();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    addTextToScroll(serverMsg, autoScroll: false);

                    if (serverMsg != null && serverMsg.StartsWith("PING"))
                    {
                        WriteMessageCommand(new Message { Command="PONG", Args = new List<string> { serverMsg.Substring(5) } });
                        addTextToScroll("UPCHECK RESP " + serverMsg.Substring(5), autoScroll: false);
                    }
                    if(serverMsg != null && serverMsg.Contains("JOIN"))
                    {
                        string[] parts = serverMsg.Split(":");
                        string result = parts[2];
                        currentChannel = CreateOrGetChannel(result);
                        MessageScrollView.Content = currentChannel.messageStack;
                    }
                });

                await Task.Delay(100);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    Channel CreateOrGetChannel(string channelName)
    {
        Channel newChannel;
        if (!Channel.ContainsChannelByName(channels, channelName))
        {
            newChannel = new Channel(channelName);
            channels.Add(newChannel);
        }
        else
        {
            newChannel = Channel.GetChannelByName(channels, channelName);
        }

        return newChannel;
    }

    string GetFormattedText(string serverMsg)
    {
        // Check if the serverMsg contains at least two colons
        int secondColonIndex = serverMsg.IndexOf(':', serverMsg.IndexOf(':') + 1);
        if (secondColonIndex != -1)
        {
            // Extract everything after the second colon
            string messageAfterSecondColon = serverMsg.Substring(secondColonIndex + 1).Trim();
            // Create a timestamp
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return $"{timestamp} {messageAfterSecondColon}";
        }
        else
        {
            // If there are less than two colons, just return the serverMsg
            return serverMsg;
        }
    }

    void addTextToScroll(string txt, bool autoScroll, Color? optionalColor = default)
    {
        var messageLabel = new Label
        {
            FontFamily = "JetBrainsMonoMedium",
            Text = txt

        };

        if(optionalColor != default)
        { 
            messageLabel.TextColor = optionalColor;
        }

        currentChannel.messageStack.Add(messageLabel);

        if (autoScroll)
        {
            currentChannel.messageStack.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await MessageScrollView.ScrollToAsync(messageLabel, ScrollToPosition.End, true);
                });

                return false;
            });
        }
    }

    //Ran as page is unloaded, to prevent multiple connections from the same IP address
    private void OnPageUnloaded(object sender, EventArgs e)
    {
        WriteMessageCommand(new Message { Command="QUIT", Args = new List<string>{ "Goodbye" } });
    }
    private void Send_Clicked(object sender, EventArgs e)
    {
        string msg = MessageEntry.Text;
        MessageEntry.Text = "";
        WriteMessageCommand(CommandParser.ParseCommand(msg, ref channels,ref currentChannel));
    }


    /// <summary>
    /// Writes and flushes a command and message to the <see cref="System.Net.Sockets.TcpClient"/> network stream
    /// </summary>
    void WriteMessageCommand(Message m)
    {
        string fullmsg = m.Command;
        if(m.Args.Count > 0)
        {
            fullmsg += " " + string.Join(" ", m.Args);
        }

        addTextToScroll(fullmsg, autoScroll: true, new Color(0, 0, 255));

        Debug.WriteLine("sent message: " + fullmsg);
        Debug.WriteLine("Context is: " + channelCtx);
        writer.WriteLine(fullmsg + "\r\n");
        writer.Flush();
    }

}

} */

public class Channel : INotifyPropertyChanged
{
    public string Name { get; set; }
    private ObservableCollection<string> _messages;
    public ObservableCollection<string> Messages
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
        Messages = new ObservableCollection<string>();
    }

    public void AddMessage(string message)
    {
        Messages.Add(message);
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

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


/*
public class ChannelManager
{
    private Channel _currentChannel;
    public Channel CurrentChannel
    {
        get => _currentChannel;
        set
        {
            if (_currentChannel != value)
            {
                _currentChannel = value;
                OnCurrentChannelChanged();
            }
        }
    }

    public event EventHandler CurrentChannelChanged;

    protected virtual void OnCurrentChannelChanged()
    {
        CurrentChannelChanged?.Invoke(this, EventArgs.Empty);
    }
}
*/
