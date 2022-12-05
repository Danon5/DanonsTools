namespace DanonsTools.ECSLayer
{
    public interface IRunSystem : ISystem
    {
        public void Run(in World world);
    }
}