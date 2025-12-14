using System.Collections.Generic;

namespace NgoUyenNguyen
{
    public partial class EventBus
    {
        internal HashSet<EventBus> Children { get; } = new();
        internal EventBus Parent { get; private set; }
        internal ServiceLocator Container { get; set; }

        internal void AttachTo(EventBus parent)
        {
            Detach();
            Parent = parent;
            parent?.Children.Add(this);
        }

        internal void Detach()
        {
            Parent?.Children.Remove(this);
            Parent = null;
        }
    }
}
