using UnityEngine;
using UnityEngine.Events;

namespace NgoUyenNguyen
{
    [System.Serializable]
    public class Observer<T>
    {
        [SerializeField] private T value;
        [SerializeField] private UnityEvent<T> onValueChanged;
        
        public static implicit operator T(Observer<T> observer) => observer.Value;
         
        public T Value
        {
            get => value;
            set
            {
                if (Equals(this.value, value)) return;
                this.value = value;
                Invoke();
            }
        }
        
        public Observer(T value, UnityAction<T> callback = null)
        {
            this.value = value;
            onValueChanged = new UnityEvent<T>();
            if (callback != null) onValueChanged.AddListener(callback);
        }
        
        private void Invoke()
        {
            Debug.Log($"Invoking {onValueChanged.GetPersistentEventCount()} listeners");
            onValueChanged.Invoke(value);
        }
        
        public void AddListener(UnityAction<T> callback)
        {
            if (callback == null) return;
            onValueChanged.AddListener(callback);
        }
        
        public void RemoveListener(UnityAction<T> callback)
        {
            if (callback == null) return;
            onValueChanged.RemoveListener(callback);
        }
        
        public void RemoveAllListeners()
        {
            onValueChanged.RemoveAllListeners();
        }
    }
}
