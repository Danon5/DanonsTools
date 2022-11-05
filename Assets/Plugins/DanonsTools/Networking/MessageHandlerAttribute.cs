using System;

namespace DanonsTools.Networking
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MessageHandlerAttribute : Attribute
    {
        public Type MessageType { get; }

        public MessageHandlerAttribute(Type messageType)
        {
            MessageType = messageType;
        }
    }
}