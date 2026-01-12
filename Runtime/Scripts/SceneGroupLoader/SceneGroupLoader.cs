using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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

        private void Update() => UpdateSmoothProgress();

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
        
        private async UniTask LoadSceneGroupAsync(string groupName, bool reuseExistingScene)
        {
            for (var i = 0; i < sceneGroups.Length; i++)
            {
                if (sceneGroups[i].groupName != groupName) continue;
                await LoadSceneGroupAsync(i, reuseExistingScene);
                return;
            }
        }

        private async UniTask LoadSceneGroupAsync(int groupIndex, bool reuseExistingScene)
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
            if (oldSceneGroupIndex >= 0)
                OnSceneGroupUnloaded?.Invoke(oldSceneGroupIndex);

            await LoadSceneGroup(GetScenesToLoad(sceneToRemain));
            await SetActiveScene();
            OnSceneGroupLoaded?.Invoke(groupIndex);
            
            PostLoading();
        }

        private async UniTask LoadSceneGroup(List<string> sceneToLoad)
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

                await UniTask.WaitUntil(() =>
                {
                    Progress = (ops.Progress + opHandles.Progress) / 2f;
                    return ops.IsDone && opHandles.IsDone && SmoothProgress >= 1f;
                }, cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            await ActivateScenesAsync(opHandles, ops);
            
            foreach (var sceneName in sceneToLoad)
            {
                OnSceneLoaded?.Invoke(sceneName);
            }
        }


        private async UniTask UnloadTempSceneIfNecessary()
        {
            if (tmpSceneName == null || CurrentSceneGroupInstance.HasScene(tmpSceneName)) return;

            var op = SceneManager.UnloadSceneAsync(tmpSceneName);
            if (op == null) return;

            await UniTask.WaitUntil(() => op.isDone, cancellationToken: this.GetCancellationTokenOnDestroy());

            tmpSceneName = null;
        }


        private async UniTask ActivateScenesAsync(AsyncOperationHandleGroup opHandles, AsyncOperationGroup ops)
        {
            foreach (var op in ops.Operations)
                op.allowSceneActivation = true;

            var activatingOps = ops.Operations.ToList();
            activatingOps.AddRange(
                opHandles.Operations.Select(h => h.Result.ActivateAsync())
            );

            await UniTask.WaitUntil(() => activatingOps.All(op => op.isDone),
                cancellationToken: this.GetCancellationTokenOnDestroy());
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
                    break;
                }
                case SceneReferenceState.Addressable:
                {
                    var opHandle = Addressables.LoadSceneAsync(sceneReference.Address, LoadSceneMode.Additive,
                        activateOnLoad: false);
                    opHandles.Operations.Add(opHandle);
                    scenesLoadedByAddressable[sceneReference.Name] = opHandle;
                    break;
                }
            }
        }
        
        private void PostLoading()
        {
            IsLoading = false;
            smoothProgressUpdating = false;
        }

        private async UniTask SetActiveScene()
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
        
        private async UniTask UnloadSceneGroup(List<string> scenesToUnload)
        {
            await LoadTempSceneIfNecessary(scenesToUnload);

            var ops = new AsyncOperationGroup(scenesToUnload.Count);
            var opHandles = new AsyncOperationHandleGroup(scenesToUnload.Count);

            foreach (var scene in scenesToUnload)
                UnloadScene(scene, ops, opHandles);
            

            if (ops.Operations.Count == 0 && opHandles.Operations.Count == 0) return;

            await UniTask.WaitUntil(() => ops.IsDone && opHandles.IsDone, 
                cancellationToken: this.GetCancellationTokenOnDestroy());
            
            foreach (var scene in scenesToUnload)
            {
                OnSceneUnloaded?.Invoke(scene);
            }
        }

        
        private async UniTask LoadTempSceneIfNecessary(List<string> scenesToUnload)
        {
            if (scenesToUnload.Count != SceneManager.loadedSceneCount) return;

            var op = SceneManager.LoadSceneAsync(tmpSceneBuildIndex, LoadSceneMode.Additive);
            if (op == null) return;

            await UniTask.WaitUntil(() => op.isDone, cancellationToken: this.GetCancellationTokenOnDestroy());

            var tmpScene = SceneManager.GetSceneByBuildIndex(tmpSceneBuildIndex);
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
        }
    }
}