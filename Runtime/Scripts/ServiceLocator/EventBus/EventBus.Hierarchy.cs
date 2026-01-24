using System.Collections.Generic;

namespace NgoUyenNguyen
{
    public partial class EventBus
    {
        private readonly HashSet<EventBus> children = new();
        
        public IEnumerable<EventBus> Children => children;
        public EventBus Parent { get; private set; }
        internal ServiceLocator Container { get; set; }

        public void AttachTo(EventBus parent)
        {
            Detach();
            Parent = parent;
            parent?.children.Add(this);
        }

        public void Detach()
        {
            Parent?.children.Remove(this);
            Parent = null;
        }
    }
}
