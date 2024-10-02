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
    public class PrivmsgHandler : IMessageHandler
    {
        public void Handle(Message message, ConnectionViewModel viewModel)
        {
            Channel destination = viewModel.CurrentChannel; //default to current page
            if (message.Params[0].StartsWith('#')) //is to a channel
            {
                destination = viewModel.CreateOrGetChannel(message.Params[0]);
            }
            else
            {
                string[] sender = message.Prefix.Split('!');
                destination = viewModel.CreateOrGetChannel(sender[0]);
            }
            
            if(message.Text == null)
            {
                message.Text = message.Trailing;
            }
            viewModel.AddTextToScroll(message, destination, false);
        }
    }
}
