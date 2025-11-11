using System.Collections.Generic;

namespace NgoUyenNguyen.Behaviour.GOAP
{
    public interface IActionStrategy
    {
        bool CanPerform { get; }
        bool Complete { get; }

        void Start()
        {
            // noop
        }

        void Update(float deltaTime)
        {
            // noop
        }
        
        void Stop()
        {
            // noop
        }
    }
    
    public class AgentAction
    {
        public string Name { get; }
        public float Cost { get; private set; }

        public HashSet<AgentBelief> PreConditions { get; } = new();
        public HashSet<AgentBelief> Effects { get; } = new();

        private IActionStrategy strategy;
        public bool Complete => strategy.Complete;

        private AgentAction(string name)
        {
            Name = name;
        }

        public void Start() => strategy.Start();

        public void Update(float deltaTime)
        {
            // Check if the action can be performed and update the strategy
            if (strategy.CanPerform)
            {
                strategy.Update(deltaTime);
            }
            
            // Bail out if the strategy is still executing
            if (!strategy.Complete) return;
            
            // Apply effects
            foreach (var effect in Effects)
            {
                effect.Evaluate();
            }
        }
        
        public void Stop() => strategy.Stop();

        public class Builder
        {
            private readonly AgentAction action;

            public Builder(string name)
            {
                action = new AgentAction(name)
                {
                    Cost = 1
                };
            }

            public Builder WithCost(float cost)
            {
                action.Cost = cost;
                return this;
            }

            public Builder withStrategy(IActionStrategy strategy)
            {
                action.strategy = strategy;
                return this;
            }

            public Builder AddPreCondition(AgentBelief preCondition)
            {
                action.PreConditions.Add(preCondition);
                return this;
            }

            public Builder AddEffects(AgentBelief effect)
            {
                action.Effects.Add(effect);
                return this;
            }
            
            public AgentAction Build() => action;
        }
    }
}
