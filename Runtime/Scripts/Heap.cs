using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace NgoUyenNguyen
{
    public enum HeapType
    {
        Max,
        Min,
    }
    
    public class Heap<T> : IEnumerable<T> where T : class, IHeapItem<T>
    {
        private readonly List<T> items;
        private readonly HeapType type;

        public Heap(HeapType type = HeapType.Max)
        {
            items = new List<T>();
            this.type = type;
        }
        
        /// <summary>
        /// The number of items in the heap.
        /// </summary>
        public int Count => items.Count;
        
        /// <summary>
        /// True if the heap is empty.
        /// </summary>
        public bool IsEmpty => items.Count == 0;
        
        /// <summary>
        /// Method to clear the heap.
        /// </summary>
        public void Clear() => items.Clear();
        
        /// <summary>
        /// Convert the heap to an array.
        /// </summary>
        /// <returns></returns>
        public T[] ToArray() => items.ToArray();

        /// <summary>
        /// Determines whether the specified item exists in the heap.
        /// </summary>
        /// <param name="item">The item to locate in the heap.</param>
        /// <returns>True if the item is found in the heap; otherwise, false.</returns>
        public bool Contains(T item) => items.Contains(item);

        /// <summary>
        /// Returns the zero-based index of the first occurrence of the specified item in the heap.
        /// </summary>
        /// <param name="item">The item to locate in the heap.</param>
        /// <returns>The zero-based index of the first occurrence of the item within the heap,
        /// or -1 if the item is not found.</returns>
        public int IndexOf(T item) => items.IndexOf(item);


        /// <summary>
        /// Method to add an item to the heap.
        /// </summary>
        /// <param name="item">Item to be added to the heap</param>
        public void Push(T item)
        {
             item.HeapIndex = items.Count;
             items.Add(item);
             
             // The index of parent of items in the heap
             int parentIndex = (item.HeapIndex - 1) / 2;
             while (item.HeapIndex > 0)
             {
                 var parentItem = items[parentIndex];
                 if (IsHigherPriority(item, parentItem))
                 {
                     Swap(item, parentItem);
                 }
                 else break;

                 parentIndex = (item.HeapIndex - 1) / 2;
             }
        }

        /// <summary>
        /// Method to get the first item in the heap and remove it.
        /// </summary>
        /// <returns>First item</returns>
        public T Pop()
        {
            switch (items.Count)
            {
                case 0:
                    return default;
                case 1:
                {
                    var item = items[0];
                    items.Clear();
                    return item;
                }
            }

            var firstItem = items[0];
            items[0] = items[^1];
            items[0].HeapIndex = 0;
            items.RemoveAt(items.Count - 1);
            
            int currentIndex = 0;
            while (true)
            {
                int childIndexLeft = currentIndex * 2 + 1;
                int childIndexRight = currentIndex * 2 + 2;

                if (childIndexLeft >= items.Count) break; 

                int swapIndex = childIndexLeft;

                if (childIndexRight < items.Count && IsHigherPriority(items[childIndexRight], items[childIndexLeft]))
                {
                    swapIndex = childIndexRight;
                }

                if (IsHigherPriority(items[swapIndex], items[currentIndex]))
                {
                    Swap(items[currentIndex], items[swapIndex]);
                    currentIndex = swapIndex;
                }
                else break;
            }

            return firstItem;
        }
        
        /// <summary>
        /// Method to get the first item in the heap without removing it.
        /// </summary>
        /// <returns>First item</returns>
        public T Top() => items[0];
        
        private void Swap(T itemA, T itemB)
        {
            // Swap the items in the heap 
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;

            // Update their heap indices
            (itemA.HeapIndex, itemB.HeapIndex) = (itemB.HeapIndex, itemA.HeapIndex);
        }
        
        private bool IsHigherPriority(T a, T b)
        {
            if (type == HeapType.Min)
                return a.CompareTo(b) < 0; 
            else
                return a.CompareTo(b) > 0; 
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in items)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    
    public interface IHeapItem<in T> : IComparable<T> where T : class
    {
        int HeapIndex { get; set; }
    }
}
