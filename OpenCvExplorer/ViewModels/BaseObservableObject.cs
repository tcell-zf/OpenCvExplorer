using CommunityToolkit.Mvvm.Messaging;
using OpenCvExplorer.ViewModels.Messages;
using Wpf.Ui.Controls;

namespace OpenCvExplorer.ViewModels;

public enum ImageType
{
    Jpeg, Png, Webp, Tiff, Bmp, Other
}

public class BaseObservableObject : ObservableObject
{
    protected void ShowInformationalStatus(string title, string message)
    {
        WeakReferenceMessenger.Default.Send(new ApplicationStatusChangedMessage(new ApplicationStatus
        {
            Severity = InfoBarSeverity.Informational,
            Title = title,
            Message = message
        }));
    }

    protected void ShowSuccessStatus(string title, string message)
    {
        WeakReferenceMessenger.Default.Send(new ApplicationStatusChangedMessage(new ApplicationStatus
        {
            Severity = InfoBarSeverity.Success,
            Title = title,
            Message = message
        }));
    }

    protected void ShowWarningStatus(string title, string message)
    {
        WeakReferenceMessenger.Default.Send(new ApplicationStatusChangedMessage(new ApplicationStatus
        {
            Severity = InfoBarSeverity.Warning,
            Title = title,
            Message = message
        }));
    }

    protected void ShowErrorStatus(string title, string message)
    {
        WeakReferenceMessenger.Default.Send(new ApplicationStatusChangedMessage(new ApplicationStatus
        {
            Severity = InfoBarSeverity.Error,
            Title = title,
            Message = message
        }));
    }
}
