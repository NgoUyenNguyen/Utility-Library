using System;

namespace NgoUyenNguyen
{
    public interface IObservableProperty<T>
    {
        public T Value { get; set; }
        bool ChangeValue(T newValue);
        bool ChangeValueWithoutNotify(T newValue);
        void AddListener(Delegate callback);
        void RemoveListener(Delegate callback);
    }
}