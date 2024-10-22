using IRC.Models;
using IRC.ViewModels;


namespace IRC.MessageHandlers
{
    /// Represents the parsing logic for the IRC RPL_NAMREPLY (353) response.
    /// 
    /// Response format:
    /// 
    /// 353    RPL_NAMREPLY
    ///        "<type> <channel> :[[@|+]<nick> [[@|+]<nick> [...]]]"
    ///
    /// <type>: Indicates channel type
    ///         - '=': Public channel
    ///         - '*': Private channel
    ///         - '@': Secret channel
    /// 
    /// <channel>: The name of the channel
    ///
    /// [@|+]<nick>: Each nickname may be prefixed by:
    ///              - '@' (Operator)
    ///              - '+' (Voiced)
    ///
    /// Example:
    /// ":server 353 user = #example :@Alice +Bob Charlie"
    public class NamesHandler : IMessageHandler
    {
        public void Handle(Message message, ConnectionViewModel viewModel)
        {
            message.Text = "Users : "  + message.Trailing;
            viewModel.AddTextToScroll(message, viewModel.CurrentChannel, false);
        }
    }
}
