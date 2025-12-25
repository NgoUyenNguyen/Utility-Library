using System;
using System.Collections.Generic;

namespace NgoUyenNguyen
{
    public class SaveCache<T>
    {
        private readonly Dictionary<string, T> CachedData = new();
        private readonly object dataLock = new();
        
        /// <summary>
        /// The default slot name used for saving and loading operations within the SaveManager.
        /// This value is set to "default" and acts as the primary identifier for the save slot
        /// when no other specific slot name is provided.
        /// </summary>
        public string DefaultSlotName = "default";

        /// <summary>
        /// Represents the default saved data of type T, associated with the default save slot name.
        /// This property serves as a convenient way to get or set the data stored in the default save slot.
        /// If no data exists in the default slot, the getter will return the default value of T.
        /// When setting this property, the corresponding data in the default save slot is updated.
        /// </summary>
        public T DefaultSavedData
        {
            get
            {
                lock (dataLock)
                    return CachedData.GetValueOrDefault(DefaultSlotName);
            }
            set
            {
                lock (dataLock)
                    CachedData[DefaultSlotName] = value;
            }
        }
        
        /// <summary>
        /// Attempts to retrieve the saved data at the default slot.
        /// </summary>
        /// <param name="data">Data at the default slot</param>
        /// <returns>The default slot has data or not</returns>
        public bool TryGet(out T data) => TryGetAtSlot(DefaultSlotName, out data);
        
        /// <summary>
        /// Attempts to retrieve the saved data at the specified slot.
        /// </summary>
        /// <param name="slot">Slot to get</param>
        /// <param name="data">Data at the specific slot</param>
        /// <returns>The specific slot has data or not</returns>
        public bool TryGetAtSlot(string slot, out T data)
        {
            lock (dataLock)
                return CachedData.TryGetValue(slot, out data);
        }
        
        /// <summary>
        /// Retrieves the saved data at the default slot.
        /// </summary>
        /// <returns>Data at the default slot</returns>
        public T Get() => GetAtSlot(DefaultSlotName);
        
        /// <summary>
        /// Retrieves the saved data at the specified slot.
        /// </summary>
        /// <param name="slot">Slot to get</param>
        /// <returns>Data at the specified slot</returns>
        public T GetAtSlot(string slot)
        {
            lock (dataLock)
                return CachedData.GetValueOrDefault(slot);
        }
        
        /// <summary>
        /// Sets the saved data at the default slot.
        /// </summary>
        /// <param name="data">Data to set</param>
        public void Set(T data) => SetAtSlot(DefaultSlotName, data);
        
        /// <summary>
        /// Sets the saved data at the specified slot.
        /// </summary>
        /// <param name="slot">Slot to be set</param>
        /// <param name="data">Data to set</param>
        public void SetAtSlot(string slot, T data)
        {
            if (slot == null)
                throw new ArgumentNullException(nameof(slot));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            lock (dataLock)
                CachedData[slot] = data;
        }

        /// <summary>
        /// Clears the saved data at the default slot.
        /// </summary>
        public void Clear() => ClearAtSlot(DefaultSlotName);
        
        /// <summary>
        /// Clears the saved data at the specified slot.
        /// </summary>
        /// <param name="slot">Slot to be cleared</param>
        public void ClearAtSlot(string slot)
        {
            lock (dataLock)
                CachedData.Remove(slot);
        }
        
        /// <summary>
        /// Clears all saved data in the cache.
        /// </summary>
        public void ClearAll()
        {
            lock (dataLock)
                CachedData.Clear();
        }
    }
}