namespace NgoUyenNguyen
{
    /// <summary>
    /// Represents a context that can be used with brokers.
    /// </summary>
    public interface IBrokerContext : IReusable
    {
        void IReusable.Reset()
        {
            throw new System.NotImplementedException();
        }
    }
}