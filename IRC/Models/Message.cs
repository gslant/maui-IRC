using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace IRC.Models
{
    public class Message
    {
        public string RawMessage { get; set; }
        public string Text { get; set; }

        //Only use prefix as registered nickname of self, not required
        public string? Prefix { get; set; }
        public string Command { get; set; }
        public List<string> Params { get; set; }
        public string? Trailing { get; set; }

        public MessageType Type { get; set; }

        public Message(string text, MessageType type)
        {
            Text = text;
            Type = type;
        }

        public Message()
        { 

        }


        public enum MessageType
        {
            UserSent,    // Messages sent by the user
            Received,    // Messages received from others
            Warning,     // Warnings (e.g., notifications or alerts)
            Error,       // Error messages
            System       // System messages
        }
    }
}
