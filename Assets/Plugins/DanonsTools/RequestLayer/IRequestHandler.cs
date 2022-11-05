namespace DanonsTools.RequestLayer
{
    public interface IRequestHandler
    {
        public IRequestResult Handle(in IRequest request);
    }
}