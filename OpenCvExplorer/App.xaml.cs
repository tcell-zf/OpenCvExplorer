using DirectShowLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenCvExplorer.Services;
using OpenCvExplorer.ViewModels.Pages;
using OpenCvExplorer.ViewModels.Windows;
using OpenCvExplorer.Views.Pages;
using OpenCvExplorer.Views.Windows;
using Serilog;
using Serilog.Extensions.Logging;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Wpf.Ui;

namespace OpenCvExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        #region properties
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c =>
            {
                c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location))
                .AddJsonFile("appsettings.json", false, true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<ApplicationHostService>();

                // Configuration service
                services.AddSingleton(context.Configuration);

                // Page resolver service
                services.AddSingleton<IPageService, PageService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();

                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();

                services.AddSingleton<ImagePage>();
                services.AddSingleton<ImageViewModel>();
                services.AddSingleton<VideoPage>();

                IContentDialogService contentDialogService = new ContentDialogService();
                services.AddSingleton(contentDialogService);
                services.AddSingleton<ISnackbarService, SnackbarService>();

                VideoViewModel videoViewModel = new VideoViewModel(contentDialogService);
                var cameraNames = GetAllConnectedCameras();
                if (cameraNames != null && cameraNames.Count > 0)
                {
                    for (int i = 0; i < cameraNames.Count; i++)
                    {
                        if (videoViewModel.AllCameras == null)
                            videoViewModel.AllCameras = new List<CameraInfo>();

                        videoViewModel.AllCameras.Add(new CameraInfo() { Name = cameraNames[i], Index = i } );
                    }
                }
                services.AddSingleton(videoViewModel);
            }).Build();
        #endregion

        #region constructors
        public App()
        {
        }
        #endregion

        #region public functions
        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        public static void LoadStringResource()
        {
            string language = Settings.Default.Language;

            ResourceDictionary dict = new ResourceDictionary();
            switch (language.ToLower())
            {
                case "zh-hans":
                    dict.Source = new Uri(@"..\Assets\StringResources.zh-Hans.xaml", UriKind.Relative);
                    break;
                case "en":
                case "en-US":
                default:
                    dict.Source = new Uri(@"..\Assets\StringResources.xaml", UriKind.Relative);
                    break;
            }
            var existedDict = Current.Resources.MergedDictionaries.Where(d => d.Source.OriginalString.Contains(@"Assets\StringResources")).SingleOrDefault();
            if (existedDict != null)
                Current.Resources.MergedDictionaries.Remove(existedDict);
            Current.Resources.MergedDictionaries.Add(dict);
        }

        public static string GetStringResource(string key)
        {
            if (Current.Resources?.MergedDictionaries?.Count == 0)
                return string.Empty;
            if (Current.Resources?.MergedDictionaries[0] == null)
                return string.Empty;

            ResourceDictionary? dict = Current.Resources?.MergedDictionaries.Where(d => d.Source.OriginalString.Contains(@"Assets\StringResources")).SingleOrDefault();
            if (dict == null || !dict.Contains(key) || dict[key] == null)
                return string.Empty;

            return (string)dict[key];
        }

        public static ILogger<T> GetMicrosoftLogger<T>()
        {
            return new SerilogLoggerFactory(Log.Logger).CreateLogger<T>();
        }
        #endregion

        #region private functions
        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            LoadStringResource();
            _host.Start();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }

        private static List<string>? GetAllConnectedCameras()
        {
            try
            {
                var cameraNames = new List<string>();
                var devices = new List<DsDevice>(DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice));
                foreach (var device in devices)
                {
                    cameraNames.Add(device.Name);
                }
                return cameraNames;
            }
            catch (Exception ex)
            {
                GetMicrosoftLogger<App>().LogError(ex, "Failed to get connected cameras.");
                return null;
            }
        }
        #endregion
    }
}
