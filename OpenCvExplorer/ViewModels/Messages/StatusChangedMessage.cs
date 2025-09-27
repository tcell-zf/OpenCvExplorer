using CommunityToolkit.Mvvm.Messaging.Messages;
using Wpf.Ui.Controls;

namespace OpenCvExplorer.ViewModels.Messages
{
    public class ApplicationStatus
    {
        public InfoBarSeverity Severity { get; set; } = InfoBarSeverity.Informational;
        public string? Title { get; set; }
        public string? Message { get; set; }
    }

    public class ApplicationStatusChangedMessage : ValueChangedMessage<ApplicationStatus>
    {
        public ApplicationStatusChangedMessage(ApplicationStatus value) : base(value) { }
    }
}
