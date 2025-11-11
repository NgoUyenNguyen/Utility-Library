namespace NgoUyenNguyen.Behaviour
{
    public class BaseState
    {
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
