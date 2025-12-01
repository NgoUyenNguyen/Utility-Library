using System.Collections.Generic;
using UnityEngine;

namespace NgoUyenNguyen
{
    public class BrokerChain<TContext> : IBrokerChain<TContext> where TContext : class, IBrokerContext
    {
        private readonly IBroker<TContext>[] brokers;
        
        public BrokerChain(params IBroker<TContext>[] brokers)
        {
            this.brokers = brokers;
        }
        
        public void Execute(TContext context)
        {
            int index = -1;

            void Next()
            {
                index++;
                if (index < brokers.Length)
                {
                    brokers[index].Execute(context, Next);
                }
            }
            
            Next();
        }
    }
    
    public class DynamicBrokerChain<TContext> : IBrokerChain<TContext> where TContext : class, IBrokerContext
    {
        private readonly LinkedList<IBroker<TContext>> brokers = new();

        public DynamicBrokerChain(params IBroker<TContext>[] brokers)
        {
            foreach (var broker in brokers)
            {
                this.brokers.AddLast(broker);
            }
        }

        public void AddLast(IBroker<TContext> broker) => brokers.AddLast(broker);
        
        public void AddFirst(IBroker<TContext> broker) => brokers.AddFirst(broker);

        public void RemoveLast() => brokers.RemoveLast();

        public void RemoveFirst() => brokers.RemoveFirst();

        public void Clear() => brokers.Clear();

        public void Remove(IBroker<TContext> broker) => brokers.Remove(broker);

        public void Execute(TContext context)
        {
            if (brokers.First == null) return;
            ExecuteNode(brokers.First, context);
        }

        private void ExecuteNode(LinkedListNode<IBroker<TContext>> node, TContext context)
        {
            void Next()
            {
                var nextNode = node.Next;
                if (nextNode != null)
                    ExecuteNode(nextNode, context);
            }

            node.Value.Execute(context, Next);
        }
    }
}
