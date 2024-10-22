using IRC.Models;
using IRC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRC.MessageHandlers
{
    public class NamesHandler : IMessageHandler
    {
        public void Handle(Message message, ConnectionViewModel viewModel)
        {
            message.Text = "Users : "  + message.Params;
            viewModel.AddTextToScroll(message, viewModel.CurrentChannel, false);
        }
    }
}
