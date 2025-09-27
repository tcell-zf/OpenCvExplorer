using CommunityToolkit.Mvvm.Messaging.Messages;

namespace OpenCvExplorer.ViewModels.Messages
{
    public class PaneDisplayModeChangedMessage : ValueChangedMessage<string>
    {
        public PaneDisplayModeChangedMessage(string value) : base(value) { }
    }
}
