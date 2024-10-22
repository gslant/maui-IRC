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
            // Combine Params (joined by space) and the Trailing part, if any
            string paramsPart = message.Params != null ? string.Join("", message.Params) : string.Empty;
            if (paramsPart.StartsWith(viewModel.nick))
            {
                paramsPart = paramsPart.Substring(viewModel.nick.Length);
            }
            message.Text = string.IsNullOrEmpty(message.Trailing) ? paramsPart : $"{paramsPart}:{message.Trailing}";

            if (message.Text.StartsWith(':'))
            {
                message.Text = message.Text.Substring(1);
            }

            Channel dest = viewModel.CurrentChannel;
            if (message.Params != null && message.Params[0].StartsWith('#'))
            {
                dest = viewModel.CreateOrGetChannel(message.Params[0]);
            }
            viewModel.AddTextToScroll(message, viewModel.CurrentChannel, false);
        }
    }
}
