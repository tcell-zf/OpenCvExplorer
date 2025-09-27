using CommunityToolkit.Mvvm.Messaging;
using OpenCvExplorer.ViewModels.Messages;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace OpenCvExplorer.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        WeakReferenceMessenger.Default.Register<PaneDisplayModeChangedMessage>(this, (r, m) =>
        {
            object? mode;
            if (Enum.TryParse(typeof(NavigationViewPaneDisplayMode), m.Value, true, out mode))
            {
                PaneDisplayMode = NavigationViewPaneDisplayMode.LeftFluent;
                PaneDisplayMode = (NavigationViewPaneDisplayMode)mode;
            }
        });
        WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
        {
            SelectedMenuItemContent = App.GetStringResource("settings-tab-title");
        });
        WeakReferenceMessenger.Default.Register<ApplicationStatusChangedMessage>(this, (r, m) =>
        {
            ApplicationStatus.Severity = m.Value.Severity;
            ApplicationStatus.Title = m.Value.Title;
            ApplicationStatus.Message = m.Value.Message;

            ApplicationStatus.IsOpen = !(string.IsNullOrEmpty(ApplicationStatus.Title) && string.IsNullOrEmpty(ApplicationStatus.Message));
        });
    }

    [ObservableProperty]
    private string _applicationTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };

    [ObservableProperty]
    private NavigationViewPaneDisplayMode _paneDisplayMode = NavigationViewPaneDisplayMode.LeftFluent;

    [ObservableProperty]
    private bool? _isPaneOpen = null;

    [ObservableProperty]
    private string _selectedMenuItemContent = string.Empty;

    [ObservableProperty]
    private Visibility _logConsoleVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private ApplicationStatusViewModel _applicationStatus = new();
}

public partial class ApplicationStatusViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isOpen = false;
    [ObservableProperty]
    private InfoBarSeverity _severity = InfoBarSeverity.Informational;
    [ObservableProperty]
    private string _title = string.Empty;
    [ObservableProperty]
    private string _message = string.Empty;
}
