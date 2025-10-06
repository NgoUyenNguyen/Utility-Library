using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;

namespace NgoUyenNguyen
{
    public struct NativeHeap<T> : IEnumerable<T> where T : unmanaged, INativeHeapItem<T>
    {
        private NativeList<T> items;
        private HeapType heapType;
        
        public NativeHeap(Allocator allocator, HeapType heapType = HeapType.Min){
            items = new NativeList<T>(allocator);
            this.heapType = heapType;
        }

        /// <summary>
        /// Inserts an item into the heap while maintaining the heap property.
        /// </summary>
        /// <param name="item">The item to be inserted into the heap.</param>
        public void Push(T item)
        {
            var itemIndex = items.Length;
            item.HeapIndex = itemIndex;
            items.Add(item);
             
            while (itemIndex > 0)
            {
                var parentIndex = (itemIndex - 1) / 2;
                var parentItem = items[parentIndex];
                if (IsHigherPriority(item, parentItem))
                {
                    Swap(items[itemIndex].HeapIndex, parentItem.HeapIndex);
                }
                else break;
                
                itemIndex = parentIndex;
            }
        }

        public T Pop()
        {
            switch (items.Length)
            {
                case 0:
                    throw new InvalidOperationException("Heap is empty.");
                case 1:
                {
                    var item = items[0];
                    items.Clear();
                    return item;
                }
            }
            
            var removed = items[0];
            
            var lastItem = items[^1];
            lastItem.HeapIndex = 0;
            items[0] = lastItem;
            items.RemoveAt(items.Length - 1);
            
            int currentIndex = 0;
            while (true)
            {
                int childIndexLeft = currentIndex * 2 + 1;
                int childIndexRight = currentIndex * 2 + 2;

                if (childIndexLeft >= items.Length) break; 

                int swapIndex = childIndexLeft;

                if (childIndexRight < items.Length && IsHigherPriority(items[childIndexRight], items[childIndexLeft]))
                {
                    swapIndex = childIndexRight;
                }

                if (IsHigherPriority(items[swapIndex], items[currentIndex]))
                {
                    Swap(items[currentIndex].HeapIndex, items[swapIndex].HeapIndex);
                    currentIndex = swapIndex;
                }
                else break;
            }
            
            return removed;
        }
        
        /// <summary>
        /// Method to get the first item in the heap without removing it.
        /// </summary>
        /// <returns>First item</returns>
        public T Top() => items[0];

        private void Swap(int indexA, int indexB)
        {
            var itemA = items[indexA];
            var itemB = items[indexB];
            (itemA.HeapIndex, itemB.HeapIndex) = (itemB.HeapIndex, itemA.HeapIndex);

            items[indexA] = itemB;
            items[indexB] = itemA;
        }
        
        private bool IsHigherPriority(T a, T b)
        {
            if (heapType == HeapType.Min)
                return a.CompareTo(b) < 0; 
            else
                return a.CompareTo(b) > 0; 
        }
        
        public bool Contains(T item) => items.Contains(item);
        
        public int Count => items.Length;
        public bool IsEmpty => items.Length == 0;
        public bool IsCreated => items.IsCreated;
        public void Clear() => items.Clear();
        public void Dispose() => items.Dispose();
        public JobHandle Dispose(JobHandle inputDeps) => items.Dispose(inputDeps);
        
        
        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    
    
    
    
    
    public interface INativeHeapItem<T> : IComparable<T>, IEquatable<T> where T : unmanaged
    {
        int HeapIndex { get; set; }
    }
}
