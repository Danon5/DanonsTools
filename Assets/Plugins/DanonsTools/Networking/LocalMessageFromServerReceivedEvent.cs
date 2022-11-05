using DanonsTools.EventLayer;
using DarkRift;

namespace DanonsTools.Networking
{
    public struct LocalMessageFromServerReceivedEvent : IEvent
    {
        public Message Message { get; private set; }

        public LocalMessageFromServerReceivedEvent(in Message message)
        {
            Message = message;
        }
    }
}