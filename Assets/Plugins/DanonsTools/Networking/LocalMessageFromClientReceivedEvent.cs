using DanonsTools.EventLayer;

namespace DanonsTools.Networking
{
    public struct LocalMessageFromClientReceivedEvent : IEvent
    {
        public INetworkMessage UnpackedMessage { get; set; }
    }
}