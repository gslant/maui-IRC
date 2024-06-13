using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRC.Models;
public enum MessageType
{
    UserSent,
    Received,
    Warning,
    Error,
    System
}

public class Message
{
    public string Text { get; set; }
    public MessageType Type { get; set; }

    public Message(string text, MessageType type)
    {
        Text = text;
        Type = type;
    }
}

