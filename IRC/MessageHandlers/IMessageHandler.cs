using IRC.Models;
using IRC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRC.MessageHandlers
{
    public interface IMessageHandler
    {
        void Handle(Message message, ConnectionViewModel viewModel);
    }
}
