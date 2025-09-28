using UnityEngine;

namespace NgoUyenNguyen.GridSystem
{
    public class Cell : MonoBehaviour
    {
        /// <summary>
        /// <c>Grid</c> index of <c>Cell</c>
        /// </summary>
        [field: SerializeField, HideInInspector] public Vector2Int index { get; set; }
        [field: SerializeField, HideInInspector] public BaseGrid grid { get; set; }


        
        /// <summary>
        /// Method to remove reference to <c>Grid</c>
        /// </summary>
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