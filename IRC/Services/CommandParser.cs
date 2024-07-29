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

        public static MessageCommand ParseCommand(string text, Channel currentChannel)
        {
            MessageCommand m = new MessageCommand();
            text = text.Trim();
            m.RawMessage = text;

            if (text.StartsWith("/"))
            {
                string[] commandParts = text.Split(' ');
                m.Command = commandParts[0].Substring(1).ToUpper();
                m.Args = commandParts.Skip(1).ToList();

                if (m.Command == "MSG") m.Command = "PRIVMSG";
            }

            else
            {
                m.Command = "PRIVMSG";
                m.Args = new List<string> { currentChannel.Name, text };
            }
            return m;
        }
    }
}
