using System.ComponentModel;
using System.Net.Sockets;

namespace IRC
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

        }

        private void Freenode_Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ConnectionPage("irc.freenode.net", 6667));
        }

        private void Libera_Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ConnectionPage("irc.libera.chat", 6667));
        }

    }
}
