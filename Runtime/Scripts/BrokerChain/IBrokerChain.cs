namespace NgoUyenNguyen
{
    /// <summary>
    /// Defines a chain of brokers that are executed in sequence to process a given context.
    /// </summary>
    /// <typeparam name="TContext">The type of the context used for execution,
    /// which must be a class implementing <see cref="IBrokerContext"/>.</typeparam>
    public interface IBrokerChain<in TContext> where TContext : class, IBrokerContext
    {
        void Execute(TContext context);
    }
}