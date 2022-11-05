using DarkRift;

namespace DanonsTools.Networking
{
    public interface IClientMessageHandler : IMessageHandler
    {
        public void Handle(in Message message);
    }
}