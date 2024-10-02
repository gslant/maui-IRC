using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public DateTime Timestamp { get; set; }

        public string SenderOrCommand
        {
            get
            {
                // Show the sender's nickname for user-sent messages

                if(Prefix != null)
                {
                    return Prefix.Split("!")[0];
                }
                

                // For other message types, show the command (JOIN, QUIT, etc.)
                return Command;
            }
        }
        public Message(string text, MessageType type)
        {
            Type = type;
            Timestamp = DateTime.Now;
        }

        public Message()
        {
            Timestamp = DateTime.Now;
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
