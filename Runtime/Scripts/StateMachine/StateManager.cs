using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace NgoUyenNguyen.StateMachine
{
    /// <summary>
    /// Provides a base class for managing state transitions in a state machine, where each state is represented by an
    /// enumeration value.
    /// </summary>
    /// <remarks>This class is designed to be extended by derived classes that define specific states and their
    /// behaviors.  It manages the lifecycle of states, including entering, updating, and exiting states, as well as
    /// handling transitions between them.  The state machine operates by maintaining a dictionary of states, where each
    /// state is associated with a key of type <typeparamref name="EState"/>. The current state is updated on each frame,
    /// and transitions are triggered based on the logic defined in the current state's implementation.  This class also
    /// integrates with Unity's event system, forwarding collision and trigger events to the current state.</remarks>
    /// <typeparam name="EState">The enumeration type representing the possible states of the state machine. Each state must be a unique value of
    /// this enumeration.</typeparam>
    public abstract class StateManager<EState> : MonoBehaviour where EState : Enum
    {
        /// <summary>
        /// Dictionary to hold all states with their corresponding keys
        /// </summary>
        private Dictionary<EState, BaseState<EState>> statesDictionary = new Dictionary<EState, BaseState<EState>>();

        private BaseState<EState> _currentState;
        /// <summary>
        /// The current state of the <c>StateManager</c>
        /// </summary>
        public BaseState<EState> currentState
        {
            get => _currentState;
            private set => _currentState = value;
        }

        private BaseState<EState> stateBuffer { get; set; }

        /// <summary>
        /// Delegate fired when the state machine transitions from one state to another.
        /// </summary>
        /// <typeparam name="Estate">Enum type representing states.</typeparam>
        /// <param name="fromState">The previous state before the transition.</param>
        /// <param name="toState">The new state after the transition.</param>
        public delegate void StateChangedHandler(EState fromState, EState toState);
        /// <summary>
        /// Callback fired whenever state changes
        /// </summary>
        public event StateChangedHandler OnStateChanged;

        private bool isTransitioning;











        private void Awake()
        {
            // Initialize the states dictionary with states defined in derived classes
            InitializeStates();
            // Call the OnAwake method to allow derived classes to setup logic
            OnAwake();
        }

        private void Start()
        {
            // Set the CurrentState to the EntryState
            currentState = GetState(InitializeEntryState());
            // Assert that the current state is set before starting the state machine
            Assert.IsNotNull(_currentState, $"Current state of {this.name} must be set before starting the state machine.");
            // Initialize the state machine with the states defined in derived classes
            currentState.EnterState();
            // Call the OnStart method to allow derived classes to perform any additional setup
            OnStart();
        }

        private void Update()
        {
            // Assert that the current state is not null before updating
            Assert.IsNotNull(currentState, $"Current state of {this.name} must be not null.");

            BaseState<EState> nextState = null;

            if (stateBuffer != null)
            {
                // Next state assigned outerally
                nextState = stateBuffer;
                stateBuffer = null;
            }
            else
            {
                // Next state assigned internally
                nextState = GetState(currentState.GenerateNextState());
            }

            // Check if needing to transition to a new state or using update mathod of the current state
            if (!isTransitioning && nextState == currentState)
            {
                // If the next state is the same as the current state, meaning no transition is needed
                currentState.UpdateState();
            }
            else if (!isTransitioning)
            {
                // If a transition is needed, call the TransitionToState method
                TransitionToState(nextState);
            }
        }











        // Method to change the current state
        private void TransitionToState(BaseState<EState> nextState)
        {
            // Check if the dictionary contains the next state
            if (!IsStateDefined(nextState.stateKey)) return;


            isTransitioning = true;

            EState currentStateKey = currentState.stateKey;
            currentState.ExitState();

            currentState = nextState;
            currentState.EnterState();

            OnStateChanged?.Invoke(currentStateKey, nextState.stateKey);

            stateBuffer = null; // Clear the buffer after transition

            isTransitioning = false;
        }

        // Method to check if a state is defined in the state machine
        private bool IsStateDefined(EState stateKey)
        {
            if (statesDictionary.ContainsKey(stateKey))
            {
                return true;
            }
            else
            {
                Debug.LogError($"{name}: State {stateKey} is not defined in the state machine.");
                return false;
            }
        }













        /// <summary>
        /// Method to add <c>States</c> to <c>StateManager</c>
        /// </summary>
        /// <param name="states"><c>States</c> to be added</param>
        protected void AddStates(params BaseState<EState>[] states)
        {
            foreach (var state in states)
            {
                if (statesDictionary.ContainsKey(state.stateKey))
                {
                    Debug.LogError($"{name}: State {state.stateKey} already exists in the state machine.");
                    return;
                }
                statesDictionary.Add(state.stateKey, state);
            }
        }

        /// <summary>
        /// Method to remove <c>States</c> to <c>StateManager</c>
        /// </summary>
        /// <param name="states"><c>States</c> to be removed</param>
        protected void RemoveStates(params BaseState<EState>[] states)
        {
            foreach (var state in states)
            {
                if (!statesDictionary.ContainsKey(state.stateKey))
                {
                    Debug.LogError($"{name}: State {state.stateKey} does not exist in the state machine.");
                    return;
                }
                statesDictionary.Remove(state.stateKey);
            }
        }

        /// <summary>
        /// Method to get <c>State</c> from <c>stateKey</c>
        /// </summary>
        /// <param name="stateKey"></param>
        /// <returns></returns>
        protected BaseState<EState> GetState(EState stateKey)
        {
            if (!statesDictionary.ContainsKey(stateKey))
            {
                Debug.LogError($"{name}: State {stateKey} does not exist in the state machine.");
                return null;
            }
            return statesDictionary[stateKey];
        }




        /// <summary>
        /// Method to register all <c>States</c> to <c>StateManager</c>
        /// </summary>
        /// <remarks>
        /// Register all <c>States</c> by AddStates().
        /// </remarks>
        protected abstract void InitializeStates();
        /// <summary>
        /// Method to define the entry <c>State</c>
        /// </summary>
        /// <returns>
        /// returns <c>stateKey</c> of entry <c>State</c>
        /// </returns>
        protected abstract EState InitializeEntryState();
        /// <summary>
        /// Method is called in the Awake() method of the <c>StateManager</c> 
        /// </summary>
        protected virtual void OnAwake() { }
        /// <summary>
        /// Method is called in the Start() method of the <c>StateManager</c>
        /// </summary>
        protected virtual void OnStart() { }






        /// <summary>
        /// Method for outer to set the next state of the <c>StateManager</c>
        /// </summary>
        /// <param name="nextStateKey"><c>stateKey</c> of the next <c>State</c></param>
        public void SetNextState(EState nextStateKey)
        {
            // Check if the next state key exists in the states dictionary
            if (statesDictionary.ContainsKey(nextStateKey))
            {
                if (statesDictionary[nextStateKey] != stateBuffer)
                {
                    stateBuffer = statesDictionary[nextStateKey];
                }
            }
            else
            {
                Debug.LogError($"{name}: Cannot set next state to undefined state: {nextStateKey}");
            }
        }
    }
}
