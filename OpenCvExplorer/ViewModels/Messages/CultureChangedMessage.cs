using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Globalization;

namespace OpenCvExplorer.ViewModels.Messages
{
    public class CultureChangedMessage : ValueChangedMessage<CultureInfo>
    {
        public CultureChangedMessage(CultureInfo value) : base(value) { }
    }
}
