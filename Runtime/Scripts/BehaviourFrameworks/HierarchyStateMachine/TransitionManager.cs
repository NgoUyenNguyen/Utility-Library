using System;
using System.Collections.Generic;
using System.Threading;

namespace NgoUyenNguyen.Behaviour.HSM
{
    internal class TransitionManager
    {
        private readonly StateMachine machine;

        private ISequence sequencer;                // current phase (deactivate or activate)
        private Action nextPhase;                   // switch structure between phases
        private (State from, State to)? pending;    // coalesce a single pending request
        private State lastFrom, lastTo;
        internal CancellationTokenSource Cts;
        private readonly bool useSequencer; // set false to use parallel


        public TransitionManager(StateMachine machine, bool useSequencer)
        {
            this.machine = machine;
            this.useSequencer = useSequencer;
        }

        // Request a transition from one state to another
        public void RequestTransition(State from, State to)
        {
            if (to == null || from == to) return;
            if (sequencer != null)
            {
                pending = (from, to);
                return;
            }
            BeginTransition(from, to);
        }


        private void BeginTransition(State from, State to)
        {
            var lca = State.LowestCommonAncestor(from, to);
            
            // 1. Deactivate the "old branch"
            var exitSteps = GatherPhaseSteps(StatesToExit(from, lca), true);
            Cts?.Dispose();
            Cts = new CancellationTokenSource();
            sequencer = useSequencer
                ? new SequentialPhase(exitSteps, Cts.Token)
                : new ParallelPhase(exitSteps, Cts.Token);
            sequencer.Start();

            nextPhase = () =>
            {
                // 2. ChangeState
                machine.ChangeState(from, to);

                // 3. Activate the "new branch"
                var enterSteps = GatherPhaseSteps(StatesToEnter(to, lca), false);
                Cts?.Dispose();
                Cts = new CancellationTokenSource();
                sequencer = useSequencer
                    ? new SequentialPhase(enterSteps, Cts.Token)
                    : new ParallelPhase(enterSteps, Cts.Token);
                sequencer.Start();
            };
        }

        private void EndTransition()
        {
            sequencer = null;

            if (!pending.HasValue) return;
            
            var p = pending.Value;
            pending = null;
            BeginTransition(p.from, p.to);
        }

        public void Tick()
        {
            if (sequencer != null)
            {
                if (!sequencer.Update()) return;
                if (nextPhase != null)
                {
                    var n = nextPhase;
                    nextPhase = null;
                    n();
                }
                else
                {
                    EndTransition();
                }

                return; // while transitioning, we don't run normal updates
            }
            machine.InternalTick();
        }
        
        private static List<PhaseStep> GatherPhaseSteps(List<State> chain, bool deactivate)
        {
            var steps = new List<PhaseStep>();

            foreach (var t in chain)
            {
                foreach (var a in t.Activities)
                {
                    if (deactivate)
                    {
                        if (a.Mode == ActivityMode.Active) steps.Add(a.DeactivateAsync);
                    }
                    else
                    {
                        if (a.Mode == ActivityMode.Inactive) steps.Add(a.ActivateAsync);
                    }
                }
            }
            return steps;
        }
        
        // States to exit: from -> ... up to (but not including) lca; bottom -> up order
        private static List<State> StatesToExit(State from, State lca)
        {
            while (from.ActiveChild != null) from = from.ActiveChild;
            
            var list = new List<State>();
            for (var s = from; s != null && s != lca; s = s.Parent) list.Add(s);
            return list;
        }
        
        // States to enter: path from 'to' up to (but not including) lca; returned in enter order (top -> down)
        private static List<State> StatesToEnter(State to, State lca)
        {   
            while (to.ActiveChild != null) to = to.ActiveChild;
            
            var stack = new Stack<State>();
            for (var s = to; s != null && s != lca; s = s.Parent) stack.Push(s);
            return new List<State>(stack);
        }
    }
}