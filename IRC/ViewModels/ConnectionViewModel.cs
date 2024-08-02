using IRC.MessageHandlers;
using IRC.Models;
using IRC.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Windows.Input;


namespace IRC.ViewModels
{
    public class ConnectionViewModel : INotifyPropertyChanged
    {
        TcpClient tcpClient;
        StreamReader reader;
        StreamWriter writer;

        private string _nick = "billbob123";
        private string _username = "bill";
        private string _realName = "bill";
        private string _messageText = string.Empty;

        private readonly MessageHandlerFactory _handlerFactory;

        public string Nick
        {
            get => _nick;
            set => _nick = value;
        }

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
        public event Action<bool> MessageAdded = delegate { };

        public ConnectionViewModel(string hostname, int port)
        {
            tcpClient = new TcpClient(hostname, port);
            reader = new StreamReader(tcpClient.GetStream());
            writer = new StreamWriter(tcpClient.GetStream());

            Channels.Add(CurrentChannel);

            _handlerFactory = new MessageHandlerFactory();

            SendCommand = new Command<string>(message => OnSendCommand(message));

            InitializeConnection();
        }

        private async void InitializeConnection()
        {
            try
            {
                Message nickMsg = new Message { Command = "NICK", Params = new List<string> { _nick } };
                WriteMessageCommand(nickMsg);

                Message userMsg = new Message { Command = "USER", Params = new List<string> { _username, "0 * :", _realName } };
                WriteMessageCommand(userMsg);

                while (true)
                {
                    string? serverMsg = await reader.ReadLineAsync();
                    if (serverMsg != null)
                    {

                        Message m = MessageParser.ParseMessage(serverMsg);
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

        public void AddTextToScroll(string text, Channel destination, bool isUserMessage, MessageType type)
        {
            destination.AddMessage(text, type);
            MessageAdded?.Invoke(isUserMessage);
        }

        //Overload for if channel is not specified
        public void AddTextToScroll(string text, bool isUserMessage, MessageType type)
        {
            CurrentChannel.AddMessage(text, type);
            MessageAdded?.Invoke(isUserMessage);
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
            writer.Write(formattedMessage);
            writer.Flush();

            AddTextToScroll(fullMsg, CurrentChannel, isUserMessage: true, type: MessageType.UserSent);
        }

        private void OnSendCommand(string message)
        {
            Debug.WriteLine("Send command executed"); // Add this line for debugging
            if (!string.IsNullOrEmpty(MessageText))
            {
                Message m = CommandParser.ParseCommand(MessageText, CurrentChannel);

                if(CurrentChannel.Name == "---" && m.Command != "JOIN")
                {
                    AddTextToScroll("Please join a real channel before sending messages", CurrentChannel, isUserMessage: true, type: MessageType.Error);
                    return;
                }
                else
                {
                    WriteMessageCommand(m);
                    MessageText = string.Empty; // Clear the input field
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Cleanup()
        {
            // Logic to clean up resources, like closing the network stream
            WriteMessageCommand(new Message { Command = "QUIT", Trailing = "Goodbye" });
            reader?.Close();
            writer?.Close();
            tcpClient?.Close();
        }

    }
}
