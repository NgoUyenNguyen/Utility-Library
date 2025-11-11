using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace NgoUyenNguyen.Behaviour.HSM
{
    /// <summary>
    /// Defines a hierarchical state machine that manages states and transitions between them.
    /// </summary>
    public class StateMachine
    {
        /// <summary>
        /// Represents the root state of the hierarchical state machine. It serves as the entry point
        /// and container for the overall state structure managed by the state machine.
        /// </summary>
        public readonly State Root;

        internal readonly TransitionSequencer Sequencer;
        private bool started;

        /// <summary>
        /// Provides a mechanism for propagating notifications that operations should be canceled.
        /// This property grants access to a <see cref="System.Threading.CancellationTokenSource"/> instance
        /// used internally by the state machine to manage cancellation of transitions or operations.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource => Sequencer.Cts;

        /// <summary>
        /// Represents a hierarchical state machine (HSM) that manages state-based behavior and transitions.
        /// </summary>
        /// <param name="root">The Root State of State Hierarchy Tree</param>
        /// <param name="useSequencer">Whether Activities are executed sequential when transition between states.
        /// Set false to be executed in parallel</param>
        public StateMachine(State root, bool useSequencer = true)
        {
            Root = root;
            Wire(root, new HashSet<State>());
            Sequencer = new TransitionSequencer(this, useSequencer);
        }

        /// <summary>
        /// Starts the hierarchical state machine (HSM) by entering its root state.
        /// Ensures that the state machine begins managing and transitioning states.
        /// </summary>
        /// <remarks>
        /// Subsequent calls to this method will have no effect if the state machine has
        /// already started. The root state's Enter method is called to initialize the behavior.
        /// </remarks>
        public void Start()
        {
            if (started) return;
            started = true;
            Root.Enter();
        }

        /// <summary>
        /// Updates the hierarchical state machine (HSM) logic once per frame or fixed timestep,
        /// enabling state updates and transitions to occur based on the provided delta time.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last update, used for time-based processing.</param>
        public void Tick(float deltaTime)
        {
            if (!started) Start();
            Sequencer.Tick(deltaTime);
        }
        
        internal void InternalTick(float deltaTime) => Root.Update(deltaTime);
        
        internal void ChangeState(State from, State to)
        {
            if (from == to || from == null || to == null) return;
            
            var lca = State.LowestCommonAncestor(from, to);
            if (lca == null) return;
            
            // Exit current branch up to (but not including) LCA
            for (var s = from; s != (from != lca ? lca : lca.Parent); s = s.Parent) s.Exit();
            
            // Enter target branch from LCA down to target
            var stack = new Stack<State>();
            for (var s = to; s != (to != lca ? lca : lca.Parent); s = s.Parent) stack.Push(s);
            while (stack.Count > 0)
            {
                var s = stack.Pop();
                if (s != to) s.Enter(false);
                else s.Enter();
            }
        }
        
        private void Wire(State s, HashSet<State> visited)
        {
            if (s == null) return;
            if (!visited.Add(s)) return; // State is already wired
            
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var machineField = typeof(State).GetField("Machine", flags);
            if (machineField != null) machineField.SetValue(s, this);

            foreach (var fld in s.GetType().GetFields(flags))
            {
                if (!typeof(State).IsAssignableFrom(fld.FieldType) || fld.Name == "Parent") continue;

                if (fld.GetValue(s) is not State child) continue;
                if (!ReferenceEquals(child.Parent, s)) continue;
                Wire(child, visited);
            }
        }
    }
}