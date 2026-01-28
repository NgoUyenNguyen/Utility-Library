
namespace NgoUyenNguyen
{
    /// <summary>
    /// Represents a chain of brokers, designed to process a context by passing it sequentially
    /// through each broker in the chain.
    /// </summary>
    /// <typeparam name="TContext">
    /// The type of the context to be processed, which must be a class implementing <see cref="IBrokerContext"/>.
    /// </typeparam>
    public class BrokerChain<TContext> : IBrokerChain<TContext> where TContext : class, IBrokerContext
    {
        private readonly IBroker<TContext>[] brokers;
        
        /// <summary>
        /// The number of brokers in the chain.
        /// </summary>
        public int Count => brokers.Length;
        /// <summary>
        /// True if the chain is empty, false otherwise.
        /// </summary>
        public bool IsEmpty => brokers.Length == 0;
        
        public BrokerChain(params IBroker<TContext>[] brokers)
        {
            this.brokers = brokers;
        }
        
        public void Execute(TContext context)
        {
            var index = -1;

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
}
