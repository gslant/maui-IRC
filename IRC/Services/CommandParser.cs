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
            Message m = new Message();
            text = text.Trim();
            m.RawMessage = text;

            if (text.StartsWith("/"))
            {
                string[] commandParts = text.Split(' ');
                m.Command = commandParts[0].Substring(1).ToUpper();
                m.Params = commandParts.Skip(1).ToList();

                if (m.Command == "MSG")
                {
                    m.Command = "PRIVMSG";
                    m.Params = new List<string>() { commandParts[1] };
                    m.Trailing = string.Join(" ", commandParts.Skip(2));
                }
            }

            else
            {
                m.Command = "PRIVMSG";
                m.Params = new List<string> { currentChannel.Name };
                m.Trailing = text;
            }
            return m;
        }
    }
}
