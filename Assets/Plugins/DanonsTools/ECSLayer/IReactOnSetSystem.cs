namespace DanonsTools.ECSLayer
{
    public interface IReactOnSetSystem<T1> : ISystem
    {
        public void OnSet(in World world, in Entity entity, in T1 oldComponent, in T1 newComponent);
    }
    
    public interface IReactOnSetSystem<T1, T2> : ISystem
    {
        public void OnSet(in World world, in Entity entity, in T1 oldComponent, in T1 newComponent);
        public void OnSet(in World world, in Entity entity, in T2 oldComponent, in T2 newComponent);
    }
    
    public interface IReactOnSetSystem<T1, T2, T3> : ISystem
    {
        public void OnSet(in World world, in Entity entity, in T1 oldComponent, in T1 newComponent);
        public void OnSet(in World world, in Entity entity, in T2 oldComponent, in T2 newComponent);
        public void OnSet(in World world, in Entity entity, in T3 oldComponent, in T3 newComponent);
    }
}