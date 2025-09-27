using Microsoft.Extensions.Configuration;
using OpenCvExplorer.ViewModels.Windows;
using Serilog;
using Serilog.Sinks.RichTextBox.Themes;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace OpenCvExplorer.Views.Windows;

public partial class MainWindow : INavigationWindow
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(
        MainWindowViewModel viewModel,
        IPageService pageService,
        INavigationService navigationService,
        IConfiguration configuration,
        IContentDialogService contentDialogService,
        ISnackbarService snackbarService)
    {
        viewModel.ApplicationTitle = App.GetStringResource("title");
        ViewModel = viewModel;
        DataContext = this;

        SystemThemeWatcher.Watch(this);

        InitializeComponent();
        SetPageService(pageService);

        navigationService.SetNavigationControl(RootNavigation);
        contentDialogService.SetDialogHost(RootContentDialog);
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.RichTextBox(OutputConsole, theme: RichTextBoxConsoleTheme.Colored)
            .CreateLogger();
    }

    #region INavigationWindow methods

    public INavigationView GetNavigation() => RootNavigation;

    public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

    public void SetPageService(IPageService pageService) => RootNavigation.SetPageService(pageService);

    public void ShowWindow()
    {
        Log.Logger.Information("Starting {0} app ...", ViewModel.ApplicationTitle);
        Show();
    }

    public void CloseWindow()
    {
        Log.Logger.Verbose("Stopping {0} app ...", ViewModel.ApplicationTitle);
        Close();
    }

    #endregion INavigationWindow methods

    /// <summary>
    /// Raises the closed event.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Make sure that closing this window will begin the process of closing the application.
        Application.Current.Shutdown();
    }

    private void FluentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        //if (ViewModel.PaneDisplayMode == NavigationViewPaneDisplayMode.Left
        //    || ViewModel.PaneDisplayMode == NavigationViewPaneDisplayMode.LeftMinimal)
        //{
        //    RootNavigation.IsPaneOpen = e.NewSize.Width > 1200;
        //}
        //else
        //    RootNavigation.IsPaneOpen = false;
    }

    INavigationView INavigationWindow.GetNavigation()
    {
        throw new NotImplementedException();
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }

    private void RootNavigation_SelectionChanged(NavigationView sender, RoutedEventArgs args)
    {
        if (sender != null && sender.SelectedItem != null && sender.SelectedItem.Content != null)
            ViewModel.SelectedMenuItemContent = sender.SelectedItem.Content.ToString();
    }

    private void OutputConsole_TextChanged(object sender, TextChangedEventArgs e)
    {
        OutputConsole.ScrollToEnd();
    }

    private void FluentWindow_KeyUp(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                CloseWindow();
                break;
            case Key.F12:
                ViewModel.LogConsoleVisibility = (ViewModel.LogConsoleVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed);
                if (ViewModel.LogConsoleVisibility == Visibility.Visible)
                    Log.Logger.Verbose("Show log console.");
                else
                    Log.Logger.Verbose("Hide log console.");
                break;
            default:
                break;
        }
    }
}
