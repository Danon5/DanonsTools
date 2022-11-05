namespace DanonsTools.RequestLayer
{
    public interface IRequestResult
    {
        public static EmptyRequestResult Empty { get; } = new();
        
        public T Get<T>() where T : IRequestResult => (T)this;
    }
}