using System.Collections.Generic;

namespace NgoUyenNguyen
{
    public partial class EventBus
    {
        private readonly HashSet<EventBus> children = new();

        /// <summary>
        /// Collection of child <see cref="EventBus"/> instances attached to the current <see cref="EventBus"/>.
        /// </summary>
        /// <remarks>
        /// The children represent a hierarchical relationship within the event system. This property
        /// facilitates event propagation to child buses when events are published with a propagation mode
        /// that includes downstream communication.
        /// </remarks>
        public IEnumerable<EventBus> Children => children;

        /// <summary>
        /// The parent <see cref="EventBus"/> instance in the hierarchy to which the current <see cref="EventBus"/> is attached.
        /// </summary>
        /// <remarks>
        /// This property represents the hierarchical relationship of event buses, enabling upstream propagation of events
        /// when the propagation mode requires communication to parent buses.
        /// </remarks>
        public EventBus Parent { get; private set; }
        internal ServiceLocator Container { get; set; }

        /// <summary>
        /// Attaches the current EventBus instance to a specified parent EventBus,
        /// establishing a hierarchical relationship.
        /// </summary>
        /// <param name="parent">
        /// The EventBus that will act as the parent for the current instance.
        /// Passing null will detach the current instance from its current parent without attaching to a new one.
        /// </param>
        public void AttachTo(EventBus parent)
        {
            Detach();
            Parent = parent;
            parent?.children.Add(this);
        }

        /// <summary>
        /// Detaches the current EventBus instance from its parent EventBus, if any,
        /// removing the hierarchical relationship.
        /// </summary>
        public void Detach()
        {
            Parent?.children.Remove(this);
            Parent = null;
        }
    }
}
