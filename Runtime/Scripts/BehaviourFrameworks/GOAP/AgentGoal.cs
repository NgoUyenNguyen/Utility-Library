using System.Collections.Generic;

namespace NgoUyenNguyen.Behaviour.GOAP
{
    public class AgentGoal
    {
        public string Name { get; }
        public float Priority { get; private set; }
        public HashSet<AgentBelief> DesiredEffects { get; } = new();

        private AgentGoal(string name)
        {
            Name = name;
        }
        
        public class Builder
        {
            private readonly AgentGoal goal;

            public Builder(string name)
            {
                goal = new AgentGoal(name);
            }

            public Builder WithPriority(float priority)
            {
                goal.Priority = priority;
                return this;
            }

            public Builder WithDesiredEffect(AgentBelief effect)
            {
                goal.DesiredEffects.Add(effect);
                return this;
            }
            
            public AgentGoal Build() => goal;
        }
    }
}
