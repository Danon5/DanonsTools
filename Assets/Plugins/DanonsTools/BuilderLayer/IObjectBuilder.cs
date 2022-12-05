namespace DanonsTools.BuilderLayer
{
    public interface IObjectBuilder<out T> : IBuilder
    {
        public T Build();
    }
}