using System.Linq;
using UnityEngine;

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

        private void Awake()
        {
            AttachEventBus();
        }

        private void AttachEventBus()
        {
            if (this == global) return;
            
            var parent = sceneContainers.ContainsValue(this) ? global?.EventBus : FindParentBus();
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
