namespace DanonsTools.BuilderLayer
{
    public interface IBuildableObject<out T> where T : IBuilder
    {
        public T WithBuilder();
    }
}