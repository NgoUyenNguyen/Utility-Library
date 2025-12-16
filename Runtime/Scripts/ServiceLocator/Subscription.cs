using System;

namespace NgoUyenNguyen
{
    public sealed class Subscription : IDisposable
    {
        private readonly Action dispose;
        public Subscription(Action dispose) => this.dispose = dispose;
        public void Dispose() => dispose();
    }
}