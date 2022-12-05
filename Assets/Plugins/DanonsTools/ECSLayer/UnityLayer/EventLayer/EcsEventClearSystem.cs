namespace DanonsTools.ECSLayer.UnityLayer.EventLayer
{
    public sealed class EcsEventClearSystem : IRunSystem
    {
        public void Run(in World world)
        {
            world.GetData<EcsEventManager>().ClearEvents();
        }
    }
}