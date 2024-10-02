using IRC.MessageHandlers;
using IRC.Models;
using IRC.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Windows.Input;
using static IRC.Models.Message;


namespace IRC.ViewModels
{
    public class ConnectionViewModel : INotifyPropertyChanged
    {
        // TCP client and stream I/O
        public TcpClient TcpClient { get; private set; }
        private StreamReader _reader;
        private StreamWriter _writer;

        // IRC connection parameters
        public string nick { get; private set; }
        private string _username;
        private string _realName;
        private string _hostname;
        private int _port;

        private string _messageText = string.Empty;

        private readonly MessageHandlerFactory _handlerFactory;


        // UI binds
        public string MessageText
        {
            get => _messageText;
            set
            {
                _messageText = value;
                OnPropertyChanged(nameof(MessageText));
            }
        }

        private ObservableCollection<Channel> _channels = new ObservableCollection<Channel>();
        public ObservableCollection<Channel> Channels
        {
            get => _channels;
            set
            {
                _channels = value;
                OnPropertyChanged(nameof(Channels));
            }
        }

        private Channel _currentChannel = new Channel("---");
        public Channel CurrentChannel
        {
            get => _currentChannel;
            set
            {
                _currentChannel = value;
                OnPropertyChanged(nameof(CurrentChannel));
            }
        }

        public ICommand SendCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        // Empty delegate so no need to check for subscribers before publishing
        public event Action MessageAdded = delegate { };

        public ConnectionViewModel(string hostname, int port, string nickname, string username, string realname, string? password)
        {
            nick = nickname;
            _username = username;
            _realName = realname;
            _hostname = hostname;
            _port = port;

            Channels.Add(CurrentChannel);

            _handlerFactory = new MessageHandlerFactory();

            SendCommand = new Command<string>(message => OnSendCommand(message));
        }

        public async Task InitializeTcpClient()
        {
            TcpClient = new TcpClient();
            int timeoutMilliseconds = 5000;
            try
            {
                // Attempt to initialize the TcpClient
                var connectTask = TcpClient.ConnectAsync(_hostname, _port);
                if(await Task.WhenAny(connectTask, Task.Delay(timeoutMilliseconds)) == connectTask)
                {
                    // Connection succeeded within timeout
                    await connectTask; // In case of exception during connection
                }
                else
                {
                    throw new TimeoutException($"Connection to {_hostname}:{_port} timed out after {timeoutMilliseconds / 1000} seconds.");
                }

                // Connection is successful, proceed with your logic
            }
            catch (SocketException ex)
            {
                // If the connection fails, throw an exception that will be caught by the ConnectionPage
                throw new Exception($"Failed to connect to {_hostname}:{_port}. Error: {ex.Message}");
            }
            catch(TimeoutException ex)
            {
                throw new Exception(ex.Message);
            }

            _reader = new StreamReader(TcpClient.GetStream());
            _writer = new StreamWriter(TcpClient.GetStream());

            ProcessServerMessagesAsync();
        }

        private async void ProcessServerMessagesAsync()
        {
            try
            {
                Message nickMsg = new Message { Command = "NICK", Params = new List<string> { nick } };
                WriteMessageCommand(nickMsg);

                Message userMsg = new Message { Command = "USER", Params = new List<string> { _username, "0 * :", _realName } };
                WriteMessageCommand(userMsg);

                while (true)
                {
                    string? serverMsg = await _reader.ReadLineAsync();
                    if (serverMsg != null)
                    {

                        Message m = MessageParser.ParseMessage(serverMsg, nick);
                        var handler = _handlerFactory.GetHandler(m.Command);
                        handler.Handle(m, this);

                        Debug.WriteLine(serverMsg);

                    }
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public Channel CreateOrGetChannel(string channelName)
        {
            if (!Channel.ContainsChannelByName(Channels, channelName))
            {
                var newChannel = new Channel(channelName);
                Channels.Add(newChannel);
                return newChannel;
            }
            else
            {
                return Channel.GetChannelByName(Channels, channelName)!;
            }
        }

        public void AddTextToScroll(Message m, Channel destination, bool isUserMessage)
        {
            destination.AddMessage(m);

            if (isUserMessage) // If user sent the message, triggers an automatic scroll to the bottom of the page
            {
                MessageAdded?.Invoke();
            }
        }

        public void WriteMessageCommand(Message m)
        {
            string fullMsg = m.Command;
            if(m.Params != null)
            {
                fullMsg += " " + string.Join(" ", m.Params);
            }
            if(m.Trailing != null)
            {
                fullMsg += " :" + string.Join(" ", m.Trailing);
            }
            Debug.WriteLine("sent message: " + fullMsg);

            string formattedMessage = fullMsg + "\r\n";
            _writer.Write(formattedMessage);
            _writer.Flush();
        }

        private void OnSendCommand(string message)
        {
            Debug.WriteLine("Send command executed"); // Add this line for debugging
            if (!string.IsNullOrEmpty(MessageText))
            {
                Message m = CommandParser.ParseCommand(MessageText, CurrentChannel);

                WriteMessageCommand(m);
                AddTextToScroll(m, CurrentChannel, isUserMessage: true);
                MessageText = string.Empty; // Clear the input field
                
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Cleanup()
        {
            // Logic to clean up resources, like closing the network stream
            try
            {
                WriteMessageCommand(new Message { Command = "QUIT", Trailing = "Goodbye" });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            _reader?.Close();
            _writer?.Close();
            TcpClient?.Close();
        }

    }
}
