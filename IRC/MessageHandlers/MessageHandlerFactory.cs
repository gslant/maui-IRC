using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRC.MessageHandlers
{
    public class MessageHandlerFactory
    {
        public IMessageHandler GetHandler(string command)
        {
            return command switch
            {
                "JOIN" => new JoinHandler(),
                "PING" => new PingHandler(),
                "PRIVMSG" => new PrivmsgHandler(),
                "366" => new NamesHandler(),
                _ => new DefaultHandler(),
            };
        }
    }
}
