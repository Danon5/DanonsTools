namespace DanonsTools.EventLayer
{
    public sealed class DefaultGlobalEventService : IGlobalEventService
    {
        public EventBus EventBus { get; } = new();
    }
}