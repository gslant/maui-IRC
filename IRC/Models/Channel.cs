using IRC.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

public class Channel : INotifyPropertyChanged
{
    public string Name { get; set; }
    private ObservableCollection<Message> _messages;
    public ObservableCollection<Message> Messages
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
        Messages = new ObservableCollection<Message>();
    }

    public void AddMessage(Message m)
    {
        Messages.Add(m);
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