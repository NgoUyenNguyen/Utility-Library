using System.Collections.Generic;

namespace NgoUyenNguyen.Behaviour
{
    public class BaseState
    {
        private readonly List<IActivity> activities = new();

        /// <summary>
        /// Represents a read-only collection of activities associated with the state.
        /// Activities define behavior or operations that are performed during the state's lifecycle,
        /// such as activation or deactivation processes.
        /// </summary>
        public IReadOnlyList<IActivity> Activities => activities;
        
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
        protected virtual void OnUpdate()
        {
        }
    }
}
