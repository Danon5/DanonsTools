namespace DanonsTools.ECSLayer
{
    public interface IReactOnRemoveSystem<T1> : ISystem
    {
        public void OnRemoved(in World world, in Entity entity, in T1 component);
    }
    
    public interface IReactOnRemove<T1, T2> : ISystem
    {
        public void OnRemoved(in World world, in Entity entity, in T1 component);
        public void OnRemoved(in World world, in Entity entity, in T2 component);
    }
    
    public interface IReactOnRemove<T1, T2, T3> : ISystem
    {
        public void OnRemoved(in World world, in Entity entity, in T1 component);
        public void OnRemoved(in World world, in Entity entity, in T2 component);
        public void OnRemoved(in World world, in Entity entity, in T3 component);
    }
}