using System.Collections.Generic;

namespace NgoUyenNguyen
{
    /// <summary>
    /// Represents a dynamic chain of brokers, providing functionality to modify the chain
    /// by adding, removing, or reordering brokers at runtime. Each broker in the chain processes
    /// a context sequentially, with control potentially passing to the next broker.
    /// </summary>
    /// <typeparam name="TContext">
    /// The type of the context to be processed by the brokers, which must be a class
    /// implementing <see cref="IBrokerContext"/>.
    /// </typeparam>
    public class DynamicBrokerChain<TContext> : IBrokerChain<TContext> where TContext : class, IBrokerContext
    {
        private readonly LinkedList<IBroker<TContext>> brokers = new();
        
        /// <summary>
        /// The number of brokers currently present in the chain.
        /// </summary>
        public int Count => brokers.Count;
        /// <summary>
        /// The first broker in the chain.
        /// </summary>
        public LinkedListNode<IBroker<TContext>> First => brokers.First;
        /// <summary>
        /// The last broker in the chain.
        /// </summary>
        public LinkedListNode<IBroker<TContext>> Last => brokers.Last;
        /// <summary>
        /// True if the chain is empty, false otherwise.
        /// </summary>
        public bool IsEmpty => brokers.Count == 0;

        public DynamicBrokerChain(params IBroker<TContext>[] brokers)
        {
            foreach (var broker in brokers)
            {
                this.brokers.AddLast(broker);
            }
        }
        
        public bool Contains(IBroker<TContext> broker) => brokers.Contains(broker);

        /// <summary>
        /// Adds a broker to the end of the chain.
        /// The broker will be executed after all previously added brokers when processing the context.
        /// </summary>
        /// <param name="broker">
        /// The broker to be added. This broker must implement <see cref="IBroker{TContext}"/> to be valid.
        /// </param>
        /// <returns>
        /// A <see cref="LinkedListNode{T}"/> representing the newly added broker at the end of the chain.
        /// </returns>
        public LinkedListNode<IBroker<TContext>> AddLast(IBroker<TContext> broker) => brokers.AddLast(broker);

        /// <summary>
        /// Adds a broker to the beginning of the chain.
        /// The broker will be executed before all previously added brokers when processing the context.
        /// </summary>
        /// <param name="broker">
        /// The broker to be added. This broker must implement <see cref="IBroker{TContext}"/> to be valid.
        /// </param>
        /// <returns>
        /// A <see cref="LinkedListNode{T}"/> representing the newly added broker at the beginning of the chain.
        /// </returns>
        public LinkedListNode<IBroker<TContext>> AddFirst(IBroker<TContext> broker) => brokers.AddFirst(broker);

        /// <summary>
        /// Adds a broker to the chain immediately before the specified node.
        /// The new broker will be executed before the broker referenced by the specified node
        /// when processing the context.
        /// </summary>
        /// <returns>
        /// A <see cref="LinkedListNode{T}"/> representing the newly added broker in the chain.
        /// </returns>
        public LinkedListNode<IBroker<TContext>> AddBefore(LinkedListNode<IBroker<TContext>> node,
            IBroker<TContext> broker)
            => brokers.AddBefore(node, broker);

        /// <summary>
        /// Adds a broker immediately after the specified node in the chain.
        /// The newly added broker will be executed after the broker represented by the specified node
        /// when processing the context.
        /// </summary>
        /// <returns>
        /// A <see cref="LinkedListNode{T}"/> representing the newly added broker in the chain.
        /// </returns>
        public LinkedListNode<IBroker<TContext>> AddAfter(LinkedListNode<IBroker<TContext>> node,
            IBroker<TContext> broker)
            => brokers.AddAfter(node, broker);
        
        /// <summary>
        /// Removes the last broker from the chain.
        /// </summary>
        public void RemoveLast() => brokers.RemoveLast();

        /// <summary>
        /// Removes the first broker from the chain.
        /// </summary>
        public void RemoveFirst() => brokers.RemoveFirst();

        /// <summary>
        /// Removes a specific broker from the chain.
        /// </summary>
        /// <param name="broker">
        /// The broker to be removed.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the broker was found and removed successfully.
        /// Returns true if the broker was removed; otherwise, false.
        /// </returns>
        public bool Remove(IBroker<TContext> broker) => brokers.Remove(broker);

        /// <summary>
        /// Removes all brokers from the chain, leaving it empty.
        /// </summary>
        public void Clear() => brokers.Clear();

        public void Execute(TContext context)
        {
            if (brokers.First == null) return;
            ExecuteNode(brokers.First, context);
        }

        private static void ExecuteNode(LinkedListNode<IBroker<TContext>> node, TContext context)
        {
            node.Value.Execute(context, Next);
            return;

            void Next()
            {
                var nextNode = node.Next;
                if (nextNode != null)
                    ExecuteNode(nextNode, context);
            }
        }
    }
}