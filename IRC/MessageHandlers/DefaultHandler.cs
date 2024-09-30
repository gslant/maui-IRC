using IRC.Models;
using IRC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IRC.Models.Message;

namespace IRC.MessageHandlers
{
    public class DefaultHandler : IMessageHandler
    {
        public void Handle(Message message, ConnectionViewModel viewModel)
        {
            viewModel.AddTextToScroll(message.RawMessage, false, MessageType.Received);
        }
    }
}
