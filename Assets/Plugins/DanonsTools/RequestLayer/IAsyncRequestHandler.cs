using Cysharp.Threading.Tasks;

namespace DanonsTools.RequestLayer
{
    public interface IAsyncRequestHandler
    {
        public UniTask<IRequestResult> Handle(IAsyncRequest asyncRequest);
    }
}