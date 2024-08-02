using IRC.Models;
using IRC.ViewModels;
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

        public static Message ParseMessage(string message)
        {
            Message m = new Message();
            m.RawMessage = message;
        
            int currentPos = 0;
            string[] parsedMessage = message.Split(' ');
                
            if (parsedMessage[currentPos][0] == ':') //Message has a prefix
            {
                m.Prefix = parsedMessage[currentPos++].Substring(1); //skip :
            }
            m.Command = parsedMessage[currentPos++];

            while (currentPos != parsedMessage.Length)
            {
                if (parsedMessage[currentPos][0] == ':') // Trailing param
                {
                    string[] trailingParams = new string[parsedMessage.Length - currentPos];

                    Array.Copy(sourceArray: parsedMessage,
                        sourceIndex: currentPos,
                        destinationArray: trailingParams,
                        destinationIndex: 0,
                        length: parsedMessage.Length-currentPos);

                    trailingParams[0] = trailingParams[0].Substring(1);
                    m.Trailing = string.Join(" ", trailingParams);
                    break;
                }

                if(m.Params == null)
                {
                    m.Params = new();
                }

                m.Params.Add(parsedMessage[currentPos++]);
            }

            return m;
        }
    }
}
