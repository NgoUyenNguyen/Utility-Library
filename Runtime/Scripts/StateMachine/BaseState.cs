using System;
using UnityEngine;

namespace NgoUyenNguyen.StateMachine
{
    /// <summary>
    /// Represents a base class for defining states in a state machine, where each state is identified by a key of an
    /// enumerated type.
    /// </summary>
    /// <remarks>This class provides a framework for implementing state-specific behavior, including methods for
    /// entering, updating, and exiting a state, as well as handling Unity-specific events such as triggers and collisions.
    /// Derived classes must implement the abstract methods to define the behavior for a specific state.</remarks>
    /// <typeparam name="EState">The enumeration type used to uniquely identify each state in the state machine.</typeparam>
    public abstract class BaseState<EState> where EState : Enum
    {
        public BaseState(EState key)
        {
            stateKey = key;
        }



        /// <summary>
        /// The key that identifies the <c>State</c>
        /// </summary>
        public EState stateKey { get; }





        /// <summary>
        /// Methods called once when <c>State</c> is entered
        /// </summary>
        public abstract void EnterState();
        /// <summary>
        /// Method called every frame while in the <c>State</c>
        /// </summary>
        public abstract void UpdateState();
        /// <summary>
        /// Methods called once when <c>State</c> is exited
        /// </summary>
        public abstract void ExitState();
        /// <summary>
        /// Method to get the next <c>stateKey</c> based on the current state logic
        /// </summary>
        /// <returns><c>stateKey</c> to access next <c>State</c></returns>
        public abstract EState GenerateNextState();
    }
}
