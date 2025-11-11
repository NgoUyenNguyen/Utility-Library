using System.Collections.Generic;
using System.Linq;

namespace NgoUyenNguyen.Behaviour.HSM
{
    /// <summary>
    /// Represents an abstract base class for implementing hierarchical states in a state machine.
    /// Maintains a relationship with parent and child states and supports activities that define behavior within the state.
    /// </summary>
    public abstract class State
    {
        /// <summary>
        /// Represents the state machine associated with a hierarchical state management system.
        /// </summary>
        public readonly StateMachine Machine;

        /// <summary>
        /// Represents the parent state of the current state within a hierarchical state management system.
        /// </summary>
        public readonly State Parent;

        private State activeChild;
        private readonly List<IActivity> activities = new();

        /// <summary>
        /// Represents a read-only collection of activities associated with the state.
        /// Activities define behavior or operations that are performed during the state's lifecycle,
        /// such as activation or deactivation processes.
        /// </summary>
        public IReadOnlyList<IActivity> Activities => activities;

        /// <summary>
        /// Represents the currently active child state within the hierarchical state management system for the associated parent state.
        /// </summary>
        public State ActiveChild => activeChild;

        /// <summary>
        /// Retrieves the leaf state of the hierarchical state management system.
        /// The leaf state is determined by traversing the active child states
        /// starting from the current state, until a state with no active child is found.
        /// </summary>
        public State Leaf
        {
            get
            {
                var s = this;
                while (s.activeChild != null) s = s.activeChild;
                return s;
            }
        }

        /// <summary>
        /// Adds one or more activities to the current state.
        /// </summary>
        /// <param name="activity">An array of activities to add. Activities must implement the <see cref="IActivity"/> interface. Null entries in the array are ignored.</param>
        public void AddActivity(params IActivity[] activity)
        {
            foreach (var a in activity)
            {
                if (a != null) activities.Add(a);
            }
        }

        /// <summary>
        /// Enumerates the states from the current state up to the root state in a hierarchical state management system.
        /// </summary>
        public IEnumerable<State> PathToRoot
        {
            get
            {
                for (var s = this; s != null; s = s.Parent) yield return s;
            }
        }

        /// <summary>
        /// Represents an abstract base class for implementing hierarchical states in a state machine.
        /// Maintains a relationship with parent and child states and supports activities that define behavior within the state.
        /// </summary>
        protected State(State parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// Retrieves the initial state to be entered when this state is activated.
        /// </summary>
        /// <returns>The initial child state to enter, or null if this state does not have an initial child state defined.</returns>
        protected virtual State GetInitialState() => null;

        /// <summary>
        /// Determines if a transition to another state is required.
        /// This method is typically overridden in derived classes to define specific conditions for transitioning.
        /// </summary>
        /// <returns>The target state to transition to, or null if no transition is required.</returns>
        protected virtual State GetTransition() => null;

        /// <summary>
        /// Invoked when the state is entered. Override this method to define custom behavior that occurs
        /// when transitioning into this state.
        /// </summary>
        protected virtual void OnEnter()
        {
        }

        /// <summary>
        /// Invoked when the state is exited. Override this method to define custom behavior that occurs
        /// when transitioning into this state.
        /// </summary>
        protected virtual void OnExit()
        {
        }

        /// <summary>
        /// Updates the current state logic. This method is called during the state's update cycle.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last update call, typically used for time-dependent calculations.</param>
        protected virtual void OnUpdate(float deltaTime)
        {
        }
        
        internal void Enter(bool enterInitialState = true)
        {
            if (Parent != null) Parent.activeChild = this;
            OnEnter();

            if (!enterInitialState) return;
            var initial = GetInitialState();
            if (initial != null && initial != this)
                initial.Enter();
        }

        internal void Exit()
        {
            activeChild?.Exit();
            activeChild = null;
            OnExit();
        }

        internal void Update(float deltaTime)
        {
            var t = GetTransition();
            if (t != null)
            {
                Machine.Sequencer.RequestTransition(this, t);
                return;
            }

            activeChild?.Update(deltaTime);
            OnUpdate(deltaTime);
        }

        /// <summary>
        /// Generates a string representation of the path from the root state to the specified state.
        /// </summary>
        /// <param name="s">The target state for which the path is generated. The state must belong to a hierarchical state machine.</param>
        /// <returns>A string representing the path from the root to the target state, with state names separated by " > ".</returns>
        public static string StatePath(State s)
        {
            return string.Join(" > ", s.PathToRoot.AsEnumerable().Reverse().Select(n => n.GetType().Name));
        }

        // Compute the Lowest Common Ancestor of two states
        /// <summary>
        /// Finds the lowest common ancestor of two states in the state hierarchy.
        /// </summary>
        /// <param name="a">The first state.</param>
        /// <param name="b">The second state.</param>
        /// <returns>The lowest common ancestor of the two states, or null if no common ancestor exists.</returns>
        public static State LowestCommonAncestor(State a, State b)
        {
            // Create a set of all parents of 'a'
            var ap = new HashSet<State>();
            for (var s = a; s != null; s = s.Parent) ap.Add(s);

            // Find the first parent of 'b' that is also a parent of 'a'
            for (var s = b; s != null; s = s.Parent)
            {
                if (ap.Contains(s)) return s;
            }

            // If no common ancestor was found, return null
            return null;
        }
    }
}