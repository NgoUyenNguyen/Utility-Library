using System;

namespace NgoUyenNguyen
{
    public interface IBroker<in TContext> where TContext : class, IBrokerContext
    {
        void Execute(TContext context, Action next);
    }
}
