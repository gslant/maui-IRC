using IRC.Models;
using IRC.ViewModels;

namespace IRC.MessageHandlers
{
    public class ModeHandler : IMessageHandler
    {
        public void Handle(Message message, ConnectionViewModel viewModel)
        {
            string mode = message.Trailing == null ? message.Params[1] : message.Trailing;
            message.Text = message.Prefix + " sets mode " + mode + " on " + message.Params[0];
            viewModel.AddTextToScroll(message, viewModel.CurrentChannel, false);
        }
    }
}
