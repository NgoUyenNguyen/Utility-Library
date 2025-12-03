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
    /// A utility class for managing the loading and unloading of Unity scenes in groups.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public partial class SceneGroupLoader : MonoBehaviour
    {
        [Tooltip("Delay loading scene group")]
        [SerializeField, Range(0, 10)]
        private float delayLoading;
        
        [SerializeField] 
        private SceneGroup[] sceneGroups;
        private int currentSceneGroupIndex;
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> scenesLoadedByAddressable = new();
        private bool smoothProgressUpdating;
        private string tmpSceneName;

        private SceneGroup CurrentSceneGroupInstance => sceneGroups[currentSceneGroupIndex];
        

        private void Awake()
        {
            staticDelayLoading = delayLoading;
            currentSceneGroupIndex = -1;
            CheckSceneGroups();
        }

        private void Update()
        {
            UpdateSmoothProgress();
        }

        private void UpdateSmoothProgress()
        {
            if (!smoothProgressUpdating) return;
            SmoothProgress =
                staticDelayLoading <= 0
                    ? Progress
                    : Mathf.Lerp(SmoothProgress, Progress, Time.deltaTime / staticDelayLoading);
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
        
        private async Task LoadSceneGroupAsync(string groupName, bool reuseExistingScene)
        {
            for (var i = 0; i < sceneGroups.Length; i++)
            {
                if (sceneGroups[i].groupName != groupName) continue;
                await LoadSceneGroupAsync(i, reuseExistingScene);
                return;
            }
        }

        private async Task LoadSceneGroupAsync(int groupIndex, bool reuseExistingScene)
        {
            if (groupIndex < 0 || groupIndex >= sceneGroups.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(groupIndex));
            }
            if (IsLoading) return;
            
            PreLoading();

            var oldSceneGroupIndex = currentSceneGroupIndex;
            currentSceneGroupIndex = groupIndex;
            var sceneToUnload = new List<string>();
            var sceneToRemain = new List<string>();
            SeparateUnloadRemain(reuseExistingScene, oldSceneGroupIndex, sceneToUnload, sceneToRemain);

            await UnloadSceneGroup(sceneToUnload);
            InvokeSceneGroupUnloadedEvent(oldSceneGroupIndex);

            await LoadSceneGroup(GetScenesToLoad(sceneToRemain));
            await SetActiveScene();
            InvokeSceneGroupLoadedEvent();
            
            PostLoading();
        }

        private async Task LoadSceneGroup(List<string> sceneToLoad)
        {
            var opHandles = new AsyncOperationHandleGroup(sceneToLoad.Count);
            var ops = new AsyncOperationGroup(sceneToLoad.Count);
            foreach (var sceneRef in CurrentSceneGroupInstance.AllScenes)
            {
                if (sceneToLoad.Contains(sceneRef.Name)) 
                    LoadScene(sceneRef, ops, opHandles);
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

            await ActivateScenesAsync(opHandles, ops);
        }

        private async Task UnloadTempSceneIfNecessary()
        {
            if (tmpSceneName == null || CurrentSceneGroupInstance.HasScene(tmpSceneName)) return;
            var op = SceneManager.UnloadSceneAsync(tmpSceneName);
            if (op == null) return;
            while (true)
            {
                if (op.isDone) break;
                await Task.Delay(100);
            }
            tmpSceneName = null;
        }

        private async Task ActivateScenesAsync(AsyncOperationHandleGroup opHandles, AsyncOperationGroup ops)
        {
            foreach (var op in ops.Operations)
                op.allowSceneActivation = true;

            var activatingOps = ops.Operations.ToList();
            activatingOps.AddRange(opHandles.Operations.Select(h => h.Result.ActivateAsync()));
            
            while (true)
            {
                if (activatingOps.Any(op => !op.isDone)) await Task.Delay(100);
                else break;
            }
        }

        private List<string> GetScenesToLoad(List<string> sceneToRemain)
        {
            return (from sceneRef 
                in CurrentSceneGroupInstance.AllScenes 
                where !sceneToRemain.Contains(sceneRef.Name) && !SceneManager.GetSceneByName(sceneRef.Name).isLoaded 
                select sceneRef.Name).ToList();
        }

        private void SeparateUnloadRemain(bool reuseExistingScene,
            int oldSceneGroupIndex,
            List<string> sceneToUnload,
            List<string> sceneToRemain)
        {
            if (oldSceneGroupIndex == -1) return;
            
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var sceneName = SceneManager.GetSceneAt(i).name;
                if (reuseExistingScene)
                {
                    if (!CurrentSceneGroupInstance.HasScene(sceneName) &&
                        sceneGroups[oldSceneGroupIndex].HasScene(sceneName))
                    {
                        sceneToUnload.Add(sceneName);
                    }
                    else if (sceneGroups[oldSceneGroupIndex].HasScene(sceneName))
                    {
                        sceneToRemain.Add(sceneName);
                    }
                }
                else if (sceneGroups[oldSceneGroupIndex].HasScene(sceneName))
                {
                    sceneToUnload.Add(sceneName);
                }
            }
        }

        private void LoadScene(SceneReference sceneReference, AsyncOperationGroup ops,
            AsyncOperationHandleGroup opHandles)
        {
            switch (sceneReference.State)
            {
                case SceneReferenceState.Regular:
                {
                    var op = SceneManager.LoadSceneAsync(sceneReference.Name, LoadSceneMode.Additive);
                    if (op != null)
                    {
                        op.allowSceneActivation = false;
                        ops.Operations.Add(op);
                    }

                    InvokeSceneLoadedEvent(sceneReference);
                    break;
                }
                case SceneReferenceState.Addressable:
                {
                    var opHandle = Addressables.LoadSceneAsync(sceneReference.Address, LoadSceneMode.Additive,
                        activateOnLoad: false);
                    opHandles.Operations.Add(opHandle);
                    scenesLoadedByAddressable[sceneReference.Name] = opHandle;
                    InvokeSceneLoadedEvent(sceneReference);
                    break;
                }
            }
        }
        
        private void PostLoading()
        {
            IsLoading = false;
            smoothProgressUpdating = false;
        }

        private async Task SetActiveScene()
        {
            var activeScene = SceneManager.GetSceneByName(CurrentSceneGroupInstance.activeScene.Name);
            SceneManager.SetActiveScene(activeScene);
            await UnloadTempSceneIfNecessary();
        }

        private void PreLoading()
        {
            IsLoading = true;
            Progress = 0;
            SmoothProgress = 0;
        }
        
        private async Task UnloadSceneGroup(List<string> scenesToUnload)
        {
            await LoadTempSceneIfNecessary(scenesToUnload);
            
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
        
        private async Task LoadTempSceneIfNecessary(List<string> scenesToUnload)
        {
            if (scenesToUnload.Count != SceneManager.loadedSceneCount) return;
            
            var op = SceneManager.LoadSceneAsync(_tmpSceneBuildIndex, LoadSceneMode.Additive);
            if (op == null) return;
            while (!op.isDone) await Task.Delay(100);
            
            var tmpScene = SceneManager.GetSceneByBuildIndex(_tmpSceneBuildIndex);
            SceneManager.SetActiveScene(tmpScene);
            tmpSceneName = tmpScene.name;
        }

        private void UnloadScene(string scene, AsyncOperationGroup ops, AsyncOperationHandleGroup opsHandles)
        {
            if (scenesLoadedByAddressable.TryGetValue(scene, out var oph))
            {
                var opHandle = Addressables.UnloadSceneAsync(oph);
                scenesLoadedByAddressable.Remove(scene);
                opsHandles.Operations.Add(opHandle);
            }
            else
            {
                var op = SceneManager.UnloadSceneAsync(scene);
                ops.Operations.Add(op);
            }

            InvokeSceneUnloadedEvent(scene);
        }

        private static void InvokeSceneUnloadedEvent(string scene)
        {
            try
            {
                OnSceneUnloaded?.Invoke(scene);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        private static void InvokeSceneLoadedEvent(SceneReference sceneReference)
        {
            try
            {
                OnSceneLoaded?.Invoke(sceneReference.Name);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        private static void InvokeSceneGroupUnloadedEvent(int oldSceneGroupIndex)
        {
            try
            {
                OnSceneGroupUnloaded?.Invoke(oldSceneGroupIndex);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        private static void InvokeSceneGroupLoadedEvent()
        {
            try
            {
                OnSceneGroupLoaded?.Invoke(CurrentSceneGroupIndex);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}