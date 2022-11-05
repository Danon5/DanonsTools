using DanonsTools.EventLayer;
using DarkRift;

namespace DanonsTools.Networking
{
    public struct LocalMessageFromClientReceivedEvent : IEvent
    {
        public Message Message { get; private set; }

        public LocalMessageFromClientReceivedEvent(in Message message)
        {
            Message = message;
        }
    }
}