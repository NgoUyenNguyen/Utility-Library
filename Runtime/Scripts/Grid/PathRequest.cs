using Unity.Mathematics;

namespace NgoUyenNguyen.Grid
{
    internal struct PathRequest
    {
        public int2 From;
        public int2 To;
        public NeighborFilter Filter;
    }
}