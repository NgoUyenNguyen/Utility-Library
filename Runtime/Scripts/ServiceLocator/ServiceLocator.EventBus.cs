using System.Linq;

namespace NgoUyenNguyen
{
    public partial class ServiceLocator
    {
        private EventBus eventBus;

        /// <summary>
        /// Provides a centralized event bus mechanism, allowing the subscription, publishing, and propagation of events
        /// within a hierarchical structure of <see cref="ServiceLocator"/> instances.
        /// </summary>
        public EventBus EventBus
        {
            get
            {
                eventBus ??= new EventBus
                {
                    Container = this
                };
                return eventBus;
            }
        }

        /// <summary>
        /// Reattaches the current instance's EventBus, ensuring it is correctly connected
        /// to its parent EventBus in the hierarchy.
        /// </summary>
        public void ReattachEventBus() => AttachEventBus();

        private void Awake()
        {
            AttachEventBus();
        }

        private void AttachEventBus()
        {
            if (this == Global) return;
            
            var parent = SceneContainers.ContainsValue(this) ? Global?.EventBus : FindParentBus();
            if (parent == null) return;
            EventBus.AttachTo(parent);
            foreach (var child in EventBus.Parent.Children.ToArray())
            {
                if (child == EventBus) continue;
                child.AttachTo(child.Container.FindParentBus());
            }
        }

        private void DetachEventBus()
        {
            foreach (var child in EventBus.Children.ToArray())
            {
                child.AttachTo(EventBus.Parent);
            }

            EventBus.Detach();
        }

        private EventBus FindParentBus() => 
            TryGetNextInHierarchy(out var parent) ? parent.EventBus : null;
    }
}
