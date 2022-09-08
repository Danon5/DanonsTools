namespace DanonsTools.ContentManagement
{
    public sealed class DefaultContentService : IContentService
    {
        public IContentLoader ContentLoader { get; }

        public DefaultContentService(in IContentLoader contentLoader)
        {
            ContentLoader = contentLoader;
        }
    }
}