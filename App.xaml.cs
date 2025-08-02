using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StudentBarcodeApp.Services;
using StudentBarcodeApp.ViewModels;

namespace StudentBarcodeApp
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configure minimal logging
            services.AddLogging(builder => 
            {
                builder.SetMinimumLevel(LogLevel.Information);
                // Don't add any providers - this effectively creates a null logger
            });
            
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IBarcodeService, BarcodeService>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
