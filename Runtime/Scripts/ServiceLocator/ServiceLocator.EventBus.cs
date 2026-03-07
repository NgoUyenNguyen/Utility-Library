
using ZLinq;

namespace NgoUyenNguyen
{
    public partial class ServiceLocator : IEventBusContainer
    {
        /// <summary>
        /// Provides a centralized event bus mechanism, allowing the subscription, publishing, and propagation of events
        /// within a hierarchical structure of <see cref="ServiceLocator"/> instances.
        /// </summary>
        public EventBus EventBus { get; private set; }

        
        private void Awake()
        {
            EventBus = new EventBus(this);
            ReattachEventBus();
        }

        /// <summary>
        /// Reattaches the current instance's EventBus, ensuring it is correctly connected
        /// to its parent EventBus in the hierarchy.
        /// </summary>
        public void ReattachEventBus()
        {
            if (this == Global) return;
            
            var parent = SceneContainers.ContainsValue(this) ? Global?.EventBus : FindParentBus();
            if (parent == null) return;
            EventBus.AttachTo(parent);
            foreach (var child in EventBus.Parent.Children.AsValueEnumerable().ToArray())
            {
                if (child == EventBus) continue;
                
                var serviceLocator = child.Container as ServiceLocator;
                if (serviceLocator == null) continue;
                
                child.AttachTo(serviceLocator.FindParentBus());
            }
        }

        private void DetachEventBus()
        {
            foreach (var child in EventBus.Children.AsValueEnumerable().ToArray())
            {
                child.AttachTo(EventBus.Parent);
            }

            EventBus.Detach();
        }

        private EventBus FindParentBus() => 
            TryGetNextInHierarchy(out var parent) ? parent.EventBus : null;
    }
}
