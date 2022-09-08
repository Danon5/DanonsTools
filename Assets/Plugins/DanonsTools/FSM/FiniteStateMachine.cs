namespace DanonsTools.FSM
{
    public sealed class FiniteStateMachine : IStateMachine
    {
        public IState CurrentState { get; private set; }
        
        public void SetState(in IState state)
        {
            CurrentState?.OnExit();
            CurrentState = state;
            CurrentState.ParentStateMachine = this;
            CurrentState?.OnEnter();
        }

        public void TickCurrentState()
        {
            CurrentState?.Tick();
        }
    }
}