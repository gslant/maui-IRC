using IRC.Models;
using IRC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRC.MessageHandlers
{
    /// The JOIN response is sent by the server to indicate that a user has
    /// successfully joined a channel.
    /// 
    /// Response format:
    /// 
    /// ":<nick>!<user>@<host> JOIN :<channel>"
    ///
    /// <nick>: The nickname of the user who joined the channel.
    /// 
    /// <user>: The user identifier or ident of the user.
    /// 
    /// <host>: The hostname or IP address of the user.
    /// 
    /// <channel>: The name of the channel that the user joined, prefixed
    ///            with '#' or '&'.
    ///
    /// Example:
    /// ":billbob!billbob123@123456789 JOIN :#dub"
    public class JoinHandler : IMessageHandler
    {
        public void Handle(Message message, ConnectionViewModel viewModel)
        {
            message.doDisplay = false;
            if (message.Prefix != null && message.Prefix.Split('!')[0] == viewModel.nick) //Only switch channels if JOIN is for self
            {
                if (message.Params != null)
                {
                    viewModel.CurrentChannel = viewModel.CreateOrGetChannel(message.Params[0]);
                }
                else if(message.Trailing != null)
                {
                    viewModel.CurrentChannel = viewModel.CreateOrGetChannel(message.Trailing);
                }
            }
            else// If join is not for self, i.e a new user joins the channel
            {
                if(message.Prefix != null)
                {
                    message.Text = " has joined the channel";
                }
                viewModel.AddTextToScroll(message, viewModel.CurrentChannel, false);
            }
        }
    }
}
