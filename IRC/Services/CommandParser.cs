using IRC.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRC.Services
{
    internal class CommandParser
    {

        public static Message ParseCommand(string text, Channel currentChannel)
        {
            Message message = new Message();
            message.RawMessage = text;

            if (text == "/list")
            {
                message.Command = "LIST";
            }
            else if (text.StartsWith("/msg"))
            {
                message.Command = "PRIVMSG";
                message.Args = new List<string>() { text.Substring(text.IndexOf(' ') + 1) };
            }
            else if (text.StartsWith("/join"))
            {
                message.Command = "JOIN";

                string channelName = text.Substring(text.IndexOf(' ') + 1);
                message.Args = new List<string>() { channelName };
            }
            else
            {
                message.Command = "PRIVMSG";
                message.Args = new List<string> { currentChannel.Name, text };
            }
            return message;
        }
    }
}
