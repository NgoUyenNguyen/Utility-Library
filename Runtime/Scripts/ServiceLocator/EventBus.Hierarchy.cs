using System.Collections.Generic;

namespace NgoUyenNguyen
{
    public partial class EventBus
    {
        internal HashSet<EventBus> Children { get; } = new();
        internal EventBus Parent { get; private set; }
        internal ServiceLocator Container { get; set; }

        public void AttachTo(EventBus parent)
        {
            Detach();
            Parent = parent;
            parent?.Children.Add(this);
        }

        public void Detach()
        {
            Parent?.Children.Remove(this);
            Parent = null;
        }
    }
}
