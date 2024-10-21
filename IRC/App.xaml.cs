using IRC.ViewModels;

namespace IRC
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

    }
}
