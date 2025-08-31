using UnityEngine;

namespace NgoUyenNguyen.GridSystem
{
    public class Cell : MonoBehaviour
    {
        [field: SerializeField] public Vector2Int index { get; set; }
        [field: SerializeField] public BaseGrid grid { get; set; }



        protected virtual void OnDestroy()
        {
            RemoveGridReference();
        }

        public void RemoveGridReference()
        {
            if (grid == null) return;
            for (int i = 0; i < grid.cellMap.Length; i++)
            {
                if (grid.cellMap[i] == this)
                {
                    grid.cellMap[i] = null;
                    break;
                }
            }
        }
    }
}