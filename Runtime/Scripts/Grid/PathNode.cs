using Unity.Mathematics;

namespace NgoUyenNguyen.Grid
{
    internal struct PathNode : INativeHeapItem<PathNode>
    {
        public int HeapIndex { get; set; }
        public int2 GridIndex;
        public int2 ParentIndex;
        public bool Walkable;
        public int G;
        public int H;
        private int F => G + H;


        public int CompareTo(PathNode other)
        {
            int cmp = F.CompareTo(other.F);
            if (cmp == 0) cmp = H.CompareTo(other.H);
            return cmp;
        }

        public bool Equals(PathNode other)
        {
            return GridIndex.Equals(other.GridIndex);
        }

        public override int GetHashCode()
        {
            return GridIndex.GetHashCode();
        }
    }
}