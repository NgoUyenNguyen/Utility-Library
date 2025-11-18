using System.Collections.Generic;
using System.Reflection;

namespace NgoUyenNguyen.Behaviour.SSM
{
    /// <summary>
    /// Represents a Finite State Machine (FSM) implementation that manages a stack-based collection of states.
    /// It provides functionality to transition between states, handle state entry, exit, and update logic.
    /// </summary>
    public class StateMachine
    {
        private readonly Stack<State> stateStack = new();

        /// <summary>
        /// Determines whether the entry and exit methods for the current state
        /// should always be invoked during state transitions, regardless of the
        /// transition mechanism.
        /// When set to true, the current state's exit method will be called
        /// before pushing a new state, and its entry method will be called
        /// after popping back to it.
        /// When set to false, state entry and exit occur only during actual
        /// transitions between distinct states.
        /// Default value is false.
        /// </summary>
        public bool AlwaysEnterExit = false;
        private bool started;

        /// <summary>
        /// Represents a finite state machine (FSM) with a stack-based approach,
        /// designed to manage a sequence of states and their transitions.
        /// Provides methods for managing the entry and exit of states,
        /// updating the current state, and initializing the state machine.
        /// </summary>
        public StateMachine(params State[] states)
        {
            foreach (var state in states)
            {
                if (state == null) continue;
                Wire(state);
                stateStack.Push(state);
            }
        }

        /// <summary>
        /// Gets the current active state of the state machine.
        /// If no states are present in the stack, returns null.
        /// </summary>
        public State CurrentState => stateStack.Count > 0 ? stateStack.Peek() : null;

        /// <summary>
        /// Initializes and starts the state machine, ensuring the logic for entering
        /// the initial state is handled if the machine contains any states.
        /// Subsequent calls to this method have no effect if the state machine is already started.
        /// </summary>
        public void Start()
        {
            if (started) return;
            started = true;
            
            if (stateStack.Count == 0) return;
            CurrentState?.Enter();
        }

        /// <summary>
        /// Updates the current state of the state machine. If the state machine has not started,
        /// it initializes and begins execution. Once started, it ensures the current state's
        /// update logic is executed. If no states are in the stack, the method exits without action.
        /// </summary>
        public void Tick()
        {
            if (!started)
            {   
                Start();
                return;
            }
            
            if (stateStack.Count == 0) return;
            CurrentState?.Update();
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
