﻿using IRC.Models;
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
        private string _messageText = string.Empty;

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

            SendCommand = new Command<string>(message => OnSendCommand(message));

            InitializeConnection();
        }

        private async void InitializeConnection()
        {
            try
            {
                MessageCommand nickMsg = new MessageCommand { Command = "NICK", Args = new List<string> { _nick } };
                WriteMessageCommand(nickMsg);

                MessageCommand userMsg = new MessageCommand { Command = "USER", Args = new List<string> { _username, "0 * :", _realName } };
                WriteMessageCommand(userMsg);

                while (true)
                {
                    string? serverMsg = await reader.ReadLineAsync();
                    if (serverMsg != null)
                    {
                        if (serverMsg.StartsWith("PING"))
                        {
                            WriteMessageCommand(new MessageCommand { Command = "PONG", Args = new List<string> { serverMsg.Substring(5) } });
                        }
                        if (serverMsg.Contains("JOIN"))
                        {
                            string[] parts = serverMsg.Split(":");
                            string result = parts[2];
                            CurrentChannel = CreateOrGetChannel(result);
                        }

                        AddTextToScroll(serverMsg, isUserMessage: false, type: MessageType.Received);
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
                return Channel.GetChannelByName(Channels, channelName)!;
            }
        }

        private void AddTextToScroll(string text, bool isUserMessage, MessageType type)
        {
            CurrentChannel.AddMessage(text, type);
            MessageAdded?.Invoke(isUserMessage);
        }

        private void WriteMessageCommand(MessageCommand m)
        {
            string fullMsg = m.Command;
            if(m.Args != null)
            {
                fullMsg += " " + string.Join(" ", m.Args);
            }
            Debug.WriteLine("sent message: " + fullMsg);
            writer.WriteLine(fullMsg + "\r\n");
            writer.Flush();

            AddTextToScroll(fullMsg, isUserMessage: true, type: MessageType.UserSent);
        }

        private void OnSendCommand(string message)
        {
            Debug.WriteLine("Send command executed"); // Add this line for debugging
            if (!string.IsNullOrEmpty(MessageText))
            {
                MessageCommand m = CommandParser.ParseCommand(MessageText, CurrentChannel);

                if (m.Command == "JOIN" && m.Args.Count > 0)
                {
                    string channelName = m.Args[0];
                    CurrentChannel = CreateOrGetChannel(channelName);
                }
                if(CurrentChannel.Name == "---")
                {
                    AddTextToScroll("Please join a real channel before sending messages", isUserMessage: true, type: MessageType.Error);
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
            WriteMessageCommand(new MessageCommand { Command = "QUIT", Args = new List<string> { "Goodbye" } });
            reader?.Close();
            writer?.Close();
            tcpClient?.Close();
        }

    }
}
