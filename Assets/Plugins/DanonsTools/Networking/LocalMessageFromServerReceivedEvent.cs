using DanonsTools.EventLayer;

namespace DanonsTools.Networking
{
    public struct LocalMessageFromServerReceivedEvent : IEvent
    {
        public INetworkMessage UnpackedMessage { get; set; }
    }
}