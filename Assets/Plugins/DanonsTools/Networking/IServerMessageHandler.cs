using DarkRift;
using DarkRift.Server;

namespace DanonsTools.Networking
{
    public interface IServerMessageHandler : IMessageHandler
    {
        public void Handle(in IClient client, in Message message);
    }
}