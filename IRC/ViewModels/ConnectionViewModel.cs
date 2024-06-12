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
        private string _username = "billbob123";
        private string _realName = "billbob123";
        private string _messageText;

        public string MessageText
        {
            get => _messageText;
            set
            {
                _messageText = value;
                OnPropertyChanged(nameof(MessageText));
            }
        }

        private ObservableCollection<Channel> _channels;
        public ObservableCollection<Channel> Channels
        {
            get => _channels;
            set
            {
                _channels = value;
                OnPropertyChanged(nameof(Channels));
            }
        }

        private Channel _currentChannel;
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

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<bool> MessageAdded;

        public ConnectionViewModel(string hostname, int port)
        {
            tcpClient = new TcpClient(hostname, port);
            reader = new StreamReader(tcpClient.GetStream());
            writer = new StreamWriter(tcpClient.GetStream());

            Channels = new ObservableCollection<Channel>();
            Channel defaultChan = new Channel("default");
            Channels.Add(defaultChan);
            CurrentChannel = defaultChan;

            SendCommand = new Command<string>(message => OnSendCommand(message));

            InitializeConnection();
        }

        private async void InitializeConnection()
        {
            try
            {
                Message nickMsg = new Message { Command = "NICK", Args = new List<string> { _nick } };
                WriteMessageCommand(nickMsg);

                Message userMsg = new Message { Command = "USER", Args = new List<string> { _username, "0 * :", _realName } };
                WriteMessageCommand(userMsg);

                while (true)
                {
                    string serverMsg = await reader.ReadLineAsync();
                    if (serverMsg != null)
                    {
                        if (serverMsg.StartsWith("PING"))
                        {
                            WriteMessageCommand(new Message { Command = "PONG", Args = new List<string> { serverMsg.Substring(5) } });
                        }
                        if (serverMsg.Contains("JOIN"))
                        {
                            string[] parts = serverMsg.Split(":");
                            string result = parts[2];
                            CurrentChannel = CreateOrGetChannel(result);
                        }

                        AddTextToScroll(serverMsg, isUserMessage: false);
                    }
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private Channel CreateOrGetChannel(string channelName)
        {
            if (!Channel.ContainsChannelByName(Channels, channelName))
            {
                var newChannel = new Channel(channelName);
                Channels.Add(newChannel);
                return newChannel;
            }
            else
            {
                return Channel.GetChannelByName(Channels, channelName);
            }
        }

        private void AddTextToScroll(string text, bool isUserMessage)
        {
            CurrentChannel.AddMessage(text);
            MessageAdded?.Invoke(isUserMessage);
        }

        private void WriteMessageCommand(Message m)
        {
            string fullMsg = m.Command;
            if(m.Args != null)
            {
                fullMsg += " " + string.Join(" ", m.Args);
            }
            Debug.WriteLine("sent message: " + fullMsg);
            writer.WriteLine(fullMsg + "\r\n");
            writer.Flush();

            AddTextToScroll(fullMsg, isUserMessage: true);
        }

        private void OnSendCommand(string message)
        {
            Debug.WriteLine("Send command executed"); // Add this line for debugging
            if (!string.IsNullOrEmpty(MessageText))
            {
                Message m = CommandParser.ParseCommand(MessageText, CurrentChannel);

                if (m.Command == "JOIN" && m.Args.Count > 0)
                {
                    string channelName = m.Args[0];
                    CurrentChannel = CreateOrGetChannel(channelName);
                }

                WriteMessageCommand(m);
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
            WriteMessageCommand(new Message { Command = "QUIT", Args = new List<string> { "Goodbye" } });
            reader?.Close();
            writer?.Close();
            tcpClient?.Close();
        }

    }
}
