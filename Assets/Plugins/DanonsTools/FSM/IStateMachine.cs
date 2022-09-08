namespace DanonsTools.FSM
{
    public interface IStateMachine
    {
        public IState CurrentState { get; }
        public void SetState(in IState state);
        public void TickCurrentState();
    }
}