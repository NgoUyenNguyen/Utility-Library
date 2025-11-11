using System;

namespace NgoUyenNguyen.Behaviour.SM
{
    /// <summary>
    /// Represents a base class for defining states in a state machine, where each state is identified by a key of an
    /// enumerated type.
    /// </summary>
    /// <remarks>This class provides a framework for implementing state-specific behavior, including methods for
    /// entering, updating, and exiting a state, as well as handling Unity-specific events such as triggers and collisions.
    /// Derived classes must implement the abstract methods to define the behavior for a specific state.</remarks>
    /// <typeparam name="EState">The enumeration type used to uniquely identify each state in the state machine.</typeparam>
    public abstract class State<EState> : BaseState where EState : Enum
    {
        public State(EState key)
        {
            stateKey = key;
        }



        /// <summary>
        /// The key that identifies the <c>State</c>
        /// </summary>
        public EState stateKey { get; }

        /// <summary>
        /// Method to get the next <c>stateKey</c> based on the current state logic
        /// </summary>
        /// <returns><c>stateKey</c> to access next <c>State</c></returns>
        protected virtual EState GetTransition()
        {
            return stateKey;
        }
        
        internal void Enter() => OnEnter();
        internal void Update() => OnUpdate();
        internal void Exit() => OnExit();
        internal EState GetNextState() => GetTransition();
    }
}
