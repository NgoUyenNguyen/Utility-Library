namespace NgoUyenNguyen.Behaviour.SSM
{
    public abstract class State : BaseState
    {
        public readonly StateMachine Machine;

        protected virtual void GetTransition()
        {
        }
        
        internal void Enter() => OnEnter();
        internal void Exit() => OnExit();

        internal void Update()
        {
            GetTransition();
            if (Machine.CurrentState == this) OnUpdate();
        }
    }
}
