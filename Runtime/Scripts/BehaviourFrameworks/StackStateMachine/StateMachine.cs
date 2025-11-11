using System.Collections.Generic;
using System.Reflection;

namespace NgoUyenNguyen.Behaviour.SSM
{
    public class StateMachine
    {
        private readonly Stack<State> stateStack = new();
        public bool AlwaysEnterExit = false;
        private bool started;

        public StateMachine(params State[] states)
        {
            foreach (var state in states)
            {
                Wire(state);
                stateStack.Push(state);
            }
        }
        
        public State CurrentState => stateStack.Count > 0 ? stateStack.Peek() : null;

        public void Start()
        {
            if (started) return;
            started = true;
            
            if (stateStack.Count == 0) return;
            CurrentState.Enter();
        }
        
        public void Tick()
        {
            if (!started)
            {   
                Start();
                return;
            }
            
            if (stateStack.Count == 0) return;
            CurrentState.Update();
        }

        public void PushState(State state)
        {
            if (stateStack.Count > 0 && AlwaysEnterExit) CurrentState.Exit();
            Wire(state);
            stateStack.Push(state);
            state.Enter();
        }
        
        public void PopState()
        {
            if (stateStack.Count == 0) return;
            CurrentState.Exit();
            stateStack.Pop();
            if (stateStack.Count > 0 && AlwaysEnterExit) CurrentState.Enter();
        }

        private void Wire(State state)
        {
            if (state == null) return;
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var machineField = typeof(State).GetField("Machine", flags);
            if (machineField != null) machineField.SetValue(state, this);
        }
    }
}
