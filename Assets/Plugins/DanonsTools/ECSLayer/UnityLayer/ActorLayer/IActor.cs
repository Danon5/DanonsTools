namespace DanonsTools.ECSLayer.UnityLayer.ActorLayer
{
    public interface IActor
    {
        public void Clone(in World world, in Entity entity);
    }
}