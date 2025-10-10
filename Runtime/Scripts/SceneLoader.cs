using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace NgoUyenNguyen
{
    /// <summary>
    /// Represents a delegate that is invoked when a scene is loaded.
    /// </summary>
    /// <param name="sceneName">The name of the scene that has been loaded.</param>
    public delegate void OnSceneLoaded(string sceneName);

    /// <summary>
    /// Represents a delegate that is invoked when a scene is unloaded.
    /// </summary>
    /// <param name="sceneName">The name of the scene that has been unloaded.</param>
    public delegate void OnSceneUnloaded(string sceneName);

    /// <summary>
    /// Represents a delegate that is invoked when a scene group is loaded.
    /// </summary>
    /// <param name="index">The index of the scene group that has been loaded.</param>
    public delegate void OnSceneGroupLoaded(int index);

    /// <summary>
    /// Represents a delegate that is invoked when a scene group is unloaded.
    /// </summary>
    /// <param name="index">The index of the scene group that has been unloaded.</param>
    public delegate void OnSceneGroupUnloaded(int index);

    /// <summary>
    /// A utility class for managing the loading and unloading of Unity scenes in groups.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [Serializable]
        public struct SceneGroup
        {
            public string groupName;
            public SceneReference activeScene;
            public List<SceneReference> additiveScenes;

            public bool HasScene(string sceneName)
            {
                if (activeScene != null && activeScene.Name == sceneName) return true;
                return additiveScenes.Any(additiveScene => additiveScene != null && additiveScene.Name == sceneName);
            }
        }

        /// <summary>
        /// The singleton instance of the SceneLoader class.
        /// </summary>
        /// <remarks>
        /// This property ensures there is only one active instance of the SceneLoader at a time.
        /// If another instance is created, it will be automatically destroyed.
        /// </remarks>
        public static SceneLoader Instance { get; private set; }

        /// <summary>
        /// Event triggered each time a scene is successfully loaded.
        /// </summary>
        /// <remarks>
        /// This event provides the name of the scene that has been successfully loaded.
        /// It is executed after the scene has been added to the scene hierarchy.
        /// </remarks>
        /// <param name="sceneName">The name of the scene that has been loaded.</param>
        public event OnSceneLoaded OnSceneLoaded;

        /// <summary>
        /// Event triggered each time a scene is successfully unloaded.
        /// </summary>
        /// <remarks>
        /// This event provides the name of the scene that has been successfully unloaded.
        /// It is executed after the scene has been removed from the scene hierarchy.
        /// </remarks>
        /// <param name="sceneName">The name of the scene that has been unloaded.</param>
        public event OnSceneUnloaded OnSceneUnloaded;

        /// <summary>
        /// Event triggered when a group of scenes is successfully loaded.
        /// </summary>
        /// <remarks>
        /// This event provides the index of the scene group that has been loaded.
        /// It is triggered after all scenes in the specified group have been loaded
        /// and added to the scene hierarchy.
        /// </remarks>
        /// <param name="groupIndex">The index of the scene group that has been loaded.</param>
        public event OnSceneGroupLoaded OnSceneGroupLoaded;

        /// <summary>
        /// Event triggered when a group of scenes is successfully unloaded.
        /// </summary>
        /// <remarks>
        /// This event provides the index of the scene group that has been unloaded.
        /// It is invoked after all scenes in the specified group have been removed from the scene hierarchy.
        /// </remarks>
        /// <param name="sceneGroupIndex">The index of the scene group that has been unloaded.</param>
        public event OnSceneGroupUnloaded OnSceneGroupUnloaded;

        [Tooltip("Delay loading scene group")] [SerializeField, Range(0, 10)]
        private float delayLoading;

        [SerializeField] private SceneGroup[] sceneGroups;
        private int currentSceneGroupIndex;
        private Scene boostrapScene;
        private Dictionary<string, AsyncOperationHandle<SceneInstance>> scenesLoadedByAddressables = new();
        private bool smoothProgressUpdating;

        /// <summary>
        /// Indicates whether a scene or scene group is currently being loaded.
        /// </summary>
        /// <remarks>
        /// This property becomes true when scene loading begins and is set back to false
        /// after the loading process is complete. It can be used to determine if the
        /// application is in the process of loading scenes or scene groups.
        /// </remarks>
        public bool IsLoading { get; private set; }

        /// <summary>
        /// Represents the progress of the current scene loading operation as a value between 0 and 1.
        /// </summary>
        /// <remarks>
        /// This property is updated during the loading process and reflects the weighted progress
        /// of all asynchronous loading operations involved in the scene group being loaded.
        /// </remarks>
        public float Progress { get; private set; }

        /// <summary>
        /// Represents the smoothed loading progress for a scene group.
        /// </summary>
        /// <remarks>
        /// SmoothProgress provides a linearly interpolated progress value, gradually syncing with the actual
        /// loading progress (`Progress`) over time. It is especially useful for creating visually appealing
        /// loading animations or progress bars. The smoothing behavior is governed by the `delayLoading` property.
        /// </remarks>
        public float SmoothProgress { get; private set; }

        public float DelayLoading
        {
            get => delayLoading;
            set
            {
                if (value < 0) value = 0;
                delayLoading = value;
            }
        }


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            boostrapScene = gameObject.scene;
            CheckSceneGroups();
        }

        private void Update()
        {
            if (!smoothProgressUpdating) return;
            SmoothProgress =
                delayLoading <= 0 ? Progress : Mathf.Lerp(SmoothProgress, Progress, Time.deltaTime / delayLoading);
            if (Progress - SmoothProgress <= 0.05f) SmoothProgress = Progress;
        }

        private void CheckSceneGroups()
        {
            foreach (var sceneGroup in sceneGroups)
            {
                if (sceneGroup.activeScene.State == SceneReferenceState.Unsafe)
                {
                    throw new Exception($"Active scene of group '{sceneGroup.groupName}' is not valid!");
                }

                foreach (var additiveScene in sceneGroup.additiveScenes)
                {
                    if (additiveScene.State == SceneReferenceState.Unsafe)
                    {
                        throw new Exception($"Additive scene of group '{sceneGroup.groupName}' is not valid!");
                    }
                }
            }
        }

        /// <summary>
        /// Loads a group of scenes specified by the given group index.
        /// Handles unloading of existing scenes and loading of a new active scene along with its additive scenes.
        /// </summary>
        /// <param name="groupIndex">The index of the scene group to load. Must be within the range of available scene groups.</param>
        /// <param name="reuseExistingScene">
        /// If true, any scene that is already loaded and matches the required scenes in the target group will not be reloaded.
        /// If false, all scenes in the group will be loaded, replacing any matching existing scenes.
        /// </param>
        /// <returns>A Task representing the asynchronous operation of loading the scene group.</returns>
        public async Task LoadSceneGroup(int groupIndex, bool reuseExistingScene = true)
        {
            if (groupIndex < 0 || groupIndex >= sceneGroups.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(groupIndex));
            }

            if (IsLoading) return;
            PreLoading();

            var oldSceneGroupIndex = currentSceneGroupIndex;
            currentSceneGroupIndex = groupIndex;
            var newSceneGroup = sceneGroups[currentSceneGroupIndex];

            var sceneToUnload = new List<string>();
            var sceneToRemain = new List<string>();
            FilterScenes(reuseExistingScene, newSceneGroup, sceneToUnload, sceneToRemain);

            await UnloadSceneGroup(sceneToUnload);
            OnSceneGroupUnloaded?.Invoke(oldSceneGroupIndex);

            var opHandles = new AsyncOperationHandleGroup(1 + newSceneGroup.additiveScenes.Count - sceneToRemain.Count);
            var ops = new AsyncOperationGroup(1 + newSceneGroup.additiveScenes.Count - sceneToRemain.Count);
            if (!sceneToRemain.Contains(newSceneGroup.activeScene.Name))
            {
                LoadScene(newSceneGroup.activeScene, ops, opHandles);
            }

            foreach (var scene in newSceneGroup.additiveScenes.Where(scene => !sceneToRemain.Contains(scene.Name)))
            {
                LoadScene(scene, ops, opHandles);
            }

            if (ops.Operations.Count > 0 || opHandles.Operations.Count > 0)
            {
                smoothProgressUpdating = true;
                while (!ops.IsDone || !opHandles.IsDone || SmoothProgress < 1)
                {
                    Progress = (ops.Progress + opHandles.Progress) / 2;
                    await Task.Delay(100);
                }
            }

            PostLoading(newSceneGroup);
            OnSceneGroupLoaded?.Invoke(currentSceneGroupIndex);
        }

        private void FilterScenes(bool reuseExistingScene, SceneGroup newSceneGroup, List<string> sceneToUnload,
            List<string> sceneToRemain)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var sceneName = SceneManager.GetSceneAt(i).name;
                if (sceneName == boostrapScene.name) continue;
                if (reuseExistingScene)
                {
                    if (!newSceneGroup.HasScene(sceneName))
                    {
                        sceneToUnload.Add(sceneName);
                    }
                    else
                    {
                        sceneToRemain.Add(sceneName);
                    }
                }
                else
                {
                    sceneToUnload.Add(sceneName);
                }
            }
        }

        private void LoadScene(SceneReference sceneReference, AsyncOperationGroup ops,
            AsyncOperationHandleGroup opHandles)
        {
            if (sceneReference.State == SceneReferenceState.Regular)
            {
                var op = SceneManager.LoadSceneAsync(sceneReference.Name, LoadSceneMode.Additive);
                ops.Operations.Add(op);
                OnSceneLoaded?.Invoke(sceneReference.Name);
            }
            else if (sceneReference.State == SceneReferenceState.Addressable)
            {
                var opHandle = Addressables.LoadSceneAsync(sceneReference.Address, LoadSceneMode.Additive);
                opHandles.Operations.Add(opHandle);
                scenesLoadedByAddressables[sceneReference.Name] = opHandle;
                OnSceneLoaded?.Invoke(sceneReference.Name);
            }
        }

        private void PostLoading(SceneGroup newSceneGroup)
        {
            var activeScene = SceneManager.GetSceneByName(newSceneGroup.activeScene.Name);
            SceneManager.SetActiveScene(activeScene);
            IsLoading = false;
            smoothProgressUpdating = false;
        }

        private void PreLoading()
        {
            IsLoading = true;
            Progress = 0;
            SmoothProgress = 0;
        }


        private async Task UnloadSceneGroup(List<string> scenesToUnload)
        {
            var ops = new AsyncOperationGroup(scenesToUnload.Count);
            var opHandles = new AsyncOperationHandleGroup(scenesToUnload.Count);
            foreach (var scene in scenesToUnload)
            {
                UnloadScene(scene, ops, opHandles);
            }

            while (!ops.IsDone || !opHandles.IsDone)
            {
                await Task.Delay(100);
            }
        }

        private void UnloadScene(string scene, AsyncOperationGroup ops, AsyncOperationHandleGroup opsHandles)
        {
            if (scenesLoadedByAddressables.TryGetValue(scene, out var oph))
            {
                var opHandle = Addressables.UnloadSceneAsync(oph);
                scenesLoadedByAddressables.Remove(scene);
                opsHandles.Operations.Add(opHandle);
            }
            else
            {
                var op = SceneManager.UnloadSceneAsync(scene);
                ops.Operations.Add(op);
            }

            OnSceneUnloaded?.Invoke(scene);
        }
    }

    public readonly struct AsyncOperationGroup
    {
        public readonly List<AsyncOperation> Operations;

        public float Progress => Operations.Count == 0 ? 1 : Operations.Average(o => o.progress);
        public bool IsDone => Operations.All(o => o.isDone);

        public AsyncOperationGroup(int initialCapacity)
        {
            Operations = new List<AsyncOperation>(initialCapacity);
        }
    }

    public readonly struct AsyncOperationHandleGroup
    {
        public readonly List<AsyncOperationHandle<SceneInstance>> Operations;

        public float Progress => Operations.Count == 0 ? 1 : Operations.Average(o => o.PercentComplete);
        public bool IsDone => Operations.Count == 0 || Operations.All(o => o.IsDone);

        public AsyncOperationHandleGroup(int initialCapacity)
        {
            Operations = new List<AsyncOperationHandle<SceneInstance>>(initialCapacity);
        }
    }
}