using CommunityToolkit.Mvvm.Messaging;
using OpenCvExplorer.ViewModels.Messages;
using System.Globalization;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace OpenCvExplorer.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        [ObservableProperty]
        private CultureInfo _currentCulture = new CultureInfo("zh-Hans");

        [ObservableProperty]
        private List<string> _paneDisplayModes = new List<string>
        {
            NavigationViewPaneDisplayMode.Left.ToString(),
            NavigationViewPaneDisplayMode.LeftFluent.ToString(),
            NavigationViewPaneDisplayMode.LeftMinimal.ToString(),
            NavigationViewPaneDisplayMode.Top.ToString(),
            NavigationViewPaneDisplayMode.Bottom.ToString()
        };

        [ObservableProperty]
        private string _selectedPaneDisplayMode = NavigationViewPaneDisplayMode.LeftFluent.ToString();
        partial void OnSelectedPaneDisplayModeChanged(string value)
        {
            WeakReferenceMessenger.Default.Send(new PaneDisplayModeChangedMessage(value));
        }

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom() { }

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            AppVersion = $"{App.GetStringResource("title")} - {GetAssemblyVersion()}";

            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == ApplicationTheme.Light)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    CurrentTheme = ApplicationTheme.Light;

                    break;

                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;

                    break;
            }
        }

        [RelayCommand]
        private void OnChangeCulture(string cultureName)
        {
            Settings.Default.Language = cultureName;
            App.LoadStringResource();
            AppVersion = $"{App.GetStringResource("title")} - {GetAssemblyVersion()}";
            WeakReferenceMessenger.Default.Send(new CultureChangedMessage(new CultureInfo(cultureName)));

            //var culture = new CultureInfo(cultureName);
            //CultureInfo.CurrentCulture = culture;
            //CultureInfo.CurrentUICulture = culture;
            //CurrentCulture = culture;
        }
    }
}
