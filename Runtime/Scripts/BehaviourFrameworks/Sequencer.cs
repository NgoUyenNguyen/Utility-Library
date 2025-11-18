using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NgoUyenNguyen.Behaviour
{
    public interface ISequence
    {
        bool IsDone { get; }
        void Start();
        bool Update();
    }
    
    // One activity operation (activate OR deactivate) to run for this phase
    public delegate Task PhaseStep(CancellationToken ct);

    public class ParallelPhase : ISequence
    {
        private readonly List<PhaseStep> steps;
        private readonly CancellationToken ct;
        private List<Task> tasks;
        
        public bool IsDone { get; private set; }
        
        public ParallelPhase(List<PhaseStep> steps, CancellationToken ct)
        {
            this.steps = steps;
            this.ct = ct;
        }

        public void Start()
        {
            if (steps == null || steps.Count == 0)
            {
                IsDone = true;
                return;
            }
            
            tasks = new List<Task>(steps.Count);
            foreach (var step in steps)
            {
                tasks.Add(step(ct));
            }
        }

        public bool Update()
        {
            if (IsDone) return true;
            IsDone = tasks == null || tasks.TrueForAll(t => t.IsCompleted);
            return IsDone;
        }
    }

    public class SequentialPhase : ISequence
    {
        private readonly List<PhaseStep> steps;
        private readonly CancellationToken ct;
        private int index = -1;
        private Task current;
        
        public bool IsDone { get; private set; }

        public SequentialPhase(List<PhaseStep> steps, CancellationToken ct)
        {
            this.steps = steps;
            this.ct = ct;
        }

        public void Start() => Next();

        public bool Update()
        {
            if (IsDone) return true;
            if (current == null || current.IsCompleted) Next();
            return IsDone;
        }

        private void Next()
        {
            index++;
            if (index >= steps.Count)
            {
                IsDone = true;
                return;
            }
            current = steps[index](ct);
        }
    }
}