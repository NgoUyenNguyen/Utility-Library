using System;

namespace NgoUyenNguyen
{
    public interface IBrokerContext {}
    
    public interface IBroker<in TContext> where TContext : class, IBrokerContext
    {
        void Execute(TContext context, Action next);
    }
}
