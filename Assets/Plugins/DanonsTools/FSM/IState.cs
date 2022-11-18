namespace DanonsTools.Fsm
{
    public interface IState
    {
        public IStateMachine ParentStateMachine { get; set; }
        public void OnEnter();
        public void OnExit();
        public void Tick();
    }
}