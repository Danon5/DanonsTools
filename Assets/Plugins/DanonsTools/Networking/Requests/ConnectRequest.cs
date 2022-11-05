using DanonsTools.RequestLayer;

namespace DanonsTools.Networking.Requests
{
    public sealed class ConnectRequest : IRequest
    {
        public string Ip { get; }
        public ushort Port { get; }

        public ConnectRequest(in string ip, in ushort port)
        {
            Ip = ip;
            Port = port;
        }
    }
}