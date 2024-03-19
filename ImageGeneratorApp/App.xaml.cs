
using Storage = ImageGeneratorApp.Storage;
using Secrets = ImageGeneratorApp.Secrets;
namespace ImageGeneratorApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            Storage.SetSecureStorage("ConnectionString", Secrets.ConnectionString);
            Storage.SetSecureStorage("OpenAIKey", Secrets.OpenAIKey);
            MainPage = new NavigationPage(new MainPage());
        }

        // change this method to return true to see debug messages
        public static bool DebugMessagesOn()
        {
            return true;
        }

        // this method is used to set the size of the window
        protected override Window CreateWindow(IActivationState activationState)
        {
            Window window = base.CreateWindow(activationState);
            window.Height = 750;
            window.Width = 600;
            return window;
        }

    }
}
