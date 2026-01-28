namespace NgoUyenNguyen
{
    /// <summary>
    /// Defines a chain of brokers that are executed in sequence to process a given context.
    /// </summary>
    /// <typeparam name="TContext">The type of the context used for execution,
    /// which must be a class implementing <see cref="IBrokerContext"/>.</typeparam>
    public interface IBrokerChain<in TContext> where TContext : class, IBrokerContext
    {
        /// <summary>
        /// Executes the chain of brokers using the provided context.
        /// </summary>
        /// <typeparam name="TContext">
        /// The type of the context used for execution.
        /// </typeparam>
        /// <param name="context">
        /// The context object that will be passed through each broker in the chain during execution.
        /// </param>
        void Execute(TContext context);
    }
}