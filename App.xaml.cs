using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StudentBarcodeApp.Services;
using StudentBarcodeApp.ViewModels;

namespace StudentBarcodeApp
{
    // App bootstrap: sets up DI, then shows MainWindow. Keep this file lean.
    public partial class App : Application
    {
        // ServiceProvider stays private; we only need it to build and dispose the graph.
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Build a tiny service collection and resolve MainWindow once.
            // This avoids static singletons and keeps tests simpler.
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register services/view models. Logging is set to Information by default.
            services.AddLogging(b => b.SetMinimumLevel(LogLevel.Information));
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IBarcodeService, BarcodeService>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Dispose container so native handles (e.g., SQLite) get released.
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
