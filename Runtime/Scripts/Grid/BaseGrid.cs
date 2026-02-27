using System;
using System.Collections.Generic;
using UnityEngine;
using ZLinq;

namespace NgoUyenNguyen.Grid
{
    /// <summary>
    /// Represents a grid structure composed of cells, supporting various alignment and spacing options.
    /// </summary>
    /// <remarks>The <see cref="BaseGrid"/> class provides functionality for creating, managing, and
    /// interacting with a grid of cells. Likely a collection, 
    /// <see cref="BaseGrid"/> can access cells through index like a 2D array and iterated by foreach loop</remarks>
    public abstract partial class BaseGrid : MonoBehaviour
    {
        [SerializeField, HideInInspector] private bool prefabInitialized;

        [SerializeField, HideInInspector] private Cell[] _cellMap = Array.Empty<Cell>();

        [SerializeField, HideInInspector, Tooltip("Prefab to spawn in Grid")]
        private Cell _cellPrefab;

        [SerializeField, HideInInspector, Tooltip("Size of the Grid")]
        private Vector2Int _size;

        [SerializeField, HideInInspector, Tooltip("Size of each Cell")]
        private float _cellSize = 1;

        [SerializeField, HideInInspector, Tooltip("Define relative position of each Cell to the Grid")]
        private GridAlignment _alignment;

        [SerializeField, HideInInspector, Tooltip("Space to which the Grid belongs")]
        private GridSpace _space;

        [SerializeField, HideInInspector, Tooltip("Layout of each Cell")]
        private CellLayout _layout;


        public void CombineMesh(
            UnityEngine.Rendering.IndexFormat indexFormat = UnityEngine.Rendering.IndexFormat.UInt32)
        {
            var meshFilters = GetComponentsInChildren<MeshFilter>()
                .AsValueEnumerable()
                .Where(mf => {
                    if (mf.gameObject == gameObject) return false; 
            
                    var mr = mf.GetComponent<MeshRenderer>();
                    return mr != null 
                           && mr.enabled
                           && mr.sharedMaterials.Length > 0 
                           && mf.sharedMesh != null;
                });
            var materialGroups = new Dictionary<Material, List<CombineInstance>>();

            foreach (var mf in meshFilters)
            {
                var mr = mf.GetComponent<MeshRenderer>();
                var mesh = mf.sharedMesh;
                
                for (var subIndex = 0; subIndex < mesh.subMeshCount; subIndex++)
                {
                    if (subIndex >= mr.sharedMaterials.Length) continue;

                    var mat = mr.sharedMaterials[subIndex];

                    if (mat == null) continue;

                    var ci = new CombineInstance
                    {
                        mesh = mesh,
                        subMeshIndex = subIndex, 
                        transform = transform.worldToLocalMatrix * mf.transform.localToWorldMatrix
                    };
                    
                    materialGroups.TryAdd(mat, new List<CombineInstance>());
                    materialGroups[mat].Add(ci);
                }

                mf.GetComponent<MeshRenderer>().enabled = false;
            }

            var finalCombineInstances = new List<CombineInstance>();
            var uniqueMaterials = new List<Material>();

            foreach (var group in materialGroups)
            {
                var subMeshGroup = new Mesh
                {
                    indexFormat = indexFormat
                };

                subMeshGroup.CombineMeshes(group.Value.ToArray(), true);

                // ReSharper disable once InconsistentNaming
                var finalCI = new CombineInstance
                {
                    mesh = subMeshGroup,
                    transform = Matrix4x4.identity
                };

                finalCombineInstances.Add(finalCI);
                uniqueMaterials.Add(group.Key);
            }

            var finalMesh = new Mesh
            {
                name = "CombinedMesh",
                indexFormat = indexFormat
            };
            
            finalMesh.CombineMeshes(finalCombineInstances.ToArray(), false);
            // ReSharper disable once InconsistentNaming
            foreach (var finalCI in finalCombineInstances)
                DestroyImmediate(finalCI.mesh);

            gameObject.GetOrAddComponent<MeshFilter>().sharedMesh = finalMesh;
            gameObject.GetOrAddComponent<MeshRenderer>().sharedMaterials = uniqueMaterials.ToArray();
        }
    }
}