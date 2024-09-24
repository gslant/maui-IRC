using IRC.Models;
using IRC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRC.MessageHandlers
{
    public class JoinHandler : IMessageHandler
    {
        public void Handle(Message message, ConnectionViewModel viewModel)
        {
            if (message.Prefix != null && message.Prefix.Split('!')[0] == viewModel.Nick) //Only switch channels if JOIN is for self
            {
                if (message.Params!= null)
                {
                    viewModel.CurrentChannel = viewModel.CreateOrGetChannel(message.Params[0]);
                }
                else if(message.Trailing != null)
                {
                    viewModel.CurrentChannel = viewModel.CreateOrGetChannel(message.Trailing);
                }
            }
            else // If join is not for self, i.e a new user joins the channel, handle as default (later this can also update the user list)
            {
                var defaultHandler = new PrivmsgHandler();
                defaultHandler.Handle(message, viewModel);
            }
        }
    }
}
