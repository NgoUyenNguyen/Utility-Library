using System;

namespace NgoUyenNguyen
{
    /// <summary>
    /// Represents a broker that processes a context and allows chaining to the next broker in sequence.
    /// </summary>
    /// <typeparam name="TContext">
    /// The type of the context to be processed by the broker. Must be a class derived from <see cref="IBrokerContext"/>.
    /// </typeparam>
    public interface IBroker<in TContext> where TContext : class, IBrokerContext
    {
        void Execute(TContext context, Action next);
    }
}
