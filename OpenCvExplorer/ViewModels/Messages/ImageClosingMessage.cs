using CommunityToolkit.Mvvm.Messaging.Messages;

namespace OpenCvExplorer.ViewModels.Messages
{
    public class ImageClosingMessage : ValueChangedMessage<string>
    {
        public ImageClosingMessage(string value) : base(value) { }
    }
}
