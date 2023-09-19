using Serilog;
using System.Configuration;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            LogInitialize();
        }

        void LogInitialize()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(path: ConfigurationManager.AppSettings["LogFilePath"], rollingInterval: RollingInterval.Day)
            .CreateLogger();
        }
    }
}
