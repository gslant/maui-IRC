using IRC.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRC.Services
{
    internal class MessageParser
    {

        public static void ParseMessage(string message, ObservableCollection<Channel> channels)
        {
            int currentPos = 0;
            string[] parsedMessage = message.Split(' ');
            if (parsedMessage[currentPos][0] == ':')
            {
                currentPos++;
                //Message has a prefix
            }
            else
            {   

                //skip straight to next step
            }
        }
    }
}
