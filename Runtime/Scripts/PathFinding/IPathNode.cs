namespace NgoUyenNguyen.PathFinding
{
    internal class PathNode<T> : IHeapItem<PathNode<T>> where T : IMonoPath
    {
        public int HeapIndex { get; set; }
        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost => GCost + HCost;
        public PathNode<T> Parent { get; set; }
        public T Data { get; }
        
        public PathNode(T data)
        {
            Data = data;
        }
        
        public int CompareTo(PathNode<T> other)
        {
            int cmp = FCost.CompareTo(other.FCost);
            if (cmp == 0) cmp = HCost.CompareTo(other.HCost);
            return cmp; 
        }
    }
}
