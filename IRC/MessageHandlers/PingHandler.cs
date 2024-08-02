using IRC.Models;
using IRC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRC.MessageHandlers
{
    public class PingHandler : IMessageHandler
    {
        public void Handle(Message message, ConnectionViewModel viewModel)
        {
            Message response = new Message();
            response.Command = "PONG";
            response.Trailing = message.Trailing;
            viewModel.WriteMessageCommand(response);
        }
    }
}
