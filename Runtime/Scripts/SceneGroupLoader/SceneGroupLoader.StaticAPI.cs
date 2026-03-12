using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace NgoUyenNguyen
{
    public partial class SceneGroupLoader
    {
        private static float staticDelayLoading;
        private static int tmpSceneBuildIndex;

        internal static SceneGroupLoader Instance { get; set; }

        /// <summary>
        /// Event triggered each time a scene is successfully loaded.
        /// </summary>
        /// <remarks>
        /// This event provides the name of the scene that has been successfully loaded.
        /// It is executed after the scene has been added to the scene hierarchy.
        /// </remarks>
        /// <param name="sceneName">The name of the scene that has been loaded.</param>
        public static event OnSceneLoaded OnSceneLoaded;

        /// <summary>
        /// Event triggered each time a scene is successfully unloaded.
        /// </summary>
        /// <remarks>
        /// This event provides the name of the scene that has been successfully unloaded.
        /// It is executed after the scene has been removed from the scene hierarchy.
        /// </remarks>
        /// <param name="sceneName">The name of the scene that has been unloaded.</param>
        public static event OnSceneUnloaded OnSceneUnloaded;

        /// <summary>
        /// Event triggered when a group of scenes is successfully loaded.
        /// </summary>
        /// <remarks>
        /// This event provides the index of the scene group that has been loaded.
        /// It is triggered after all scenes in the specified group have been loaded
        /// and added to the scene hierarchy.
        /// </remarks>
        /// <param name="groupIndex">The index of the scene group that has been loaded.</param>
        public static event OnSceneGroupLoaded OnSceneGroupLoaded;

        /// <summary>
        /// Event triggered when a group of scenes is successfully unloaded.
        /// </summary>
        /// <remarks>
        /// This event provides the index of the scene group that has been unloaded.
        /// It is invoked after all scenes in the specified group have been removed from the scene hierarchy.
        /// </remarks>
        /// <param name="sceneGroupIndex">The index of the scene group that has been unloaded.</param>
        public static event OnSceneGroupUnloaded OnSceneGroupUnloaded;

        /// <summary>
        /// Indicates whether a scene or scene group is currently being loaded.
        /// </summary>
        /// <remarks>
        /// This property becomes true when scene loading begins and is set back to false
        /// after the loading process is complete. It can be used to determine if the
        /// application is in the process of loading scenes or scene groups.
        /// </remarks>
        public static bool IsLoading { get; private set; }

        /// <summary>
        /// Represents the progress of the current scene loading operation as a value between 0 and 1.
        /// </summary>
        /// <remarks>
        /// This property is updated during the loading process and reflects the weighted progress
        /// of all asynchronous loading operations involved in the scene group being loaded.
        /// </remarks>
        public static float Progress { get; private set; }

        /// <summary>
        /// Represents the smoothed loading progress for a scene group.
        /// </summary>
        /// <remarks>
        /// SmoothProgress provides a linearly interpolated progress value, gradually syncing with the actual
        /// loading progress (`Progress`) over time. It is especially useful for creating visually appealing
        /// loading animations or progress bars. The smoothing behavior is governed by the `delayLoading` property.
        /// </remarks>
        public static float SmoothProgress { get; private set; }

        /// <summary>
        /// Specifies a delay duration applied during the scene loading process.
        /// </summary>
        public static float DelayLoading
        {
            get => staticDelayLoading;
            set
            {
                if (value < 0) value = 0;
                staticDelayLoading = value;
            }
        }

        /// <summary>
        /// Gets the index of the currently loaded scene group.
        /// </summary>
        /// <remarks>
        /// This property provides the index of the scene group that is currently active in the application.
        /// It can be used to keep track of which group of scenes is currently loaded and managed.
        /// </remarks>
        public static int CurrentSceneGroupIndex => Instance.currentSceneGroupIndex;

        /// <summary>
        /// Represents the currently active <c>SceneGroup</c> in the scene group loader.
        /// </summary>
        /// <remarks>
        /// The <c>CurrentSceneGroup</c> property provides access to the active <c>SceneGroup</c>
        /// object, which contains a set of scenes that are being managed as a group.
        /// This property is useful for retrieving information about the scenes in the active scene group.
        /// </remarks>
        public static SceneGroup CurrentSceneGroup => Instance.CurrentSceneGroupInstance;

        /// <summary>
        /// Temporary scene build index to load when all current scenes need to be unloaded
        /// </summary>
        public static int TempSceneBuildIndex
        {
            get => tmpSceneBuildIndex;
            set => tmpSceneBuildIndex = Mathf.Max(0, value);
        }

        public static ObservableProperty<StatusValue> Status { get; private set; }


        /// <summary>
        /// Asynchronously loads a scene group specified by its group index.
        /// </summary>
        /// <param name="groupIndex">The index of the scene group to load.</param>
        /// <param name="delay">Delay coefficient</param>
        /// <param name="progress">
        /// An IProgress object for reporting the loading progress,
        /// represented as a value between 0 and 1.
        /// </param>
        /// <param name="smoothProgress">
        /// An IProgress object for reporting the smoothed loading progress,
        /// represented as a value between 0 and 1.
        /// </param>
        /// <param name="reuseExistingScene">
        /// Indicating whether to reuse the existing scenes
        /// if it is already loaded, instead of reloading it.
        /// </param>
        /// <param name="cancellationToken">
        /// A CancellationToken that can be used to cancel the loading process.
        /// </param>
        /// <returns>A UniTask representing the asynchronous loading operation.</returns>
        public static async UniTask LoadAsync(
            int groupIndex,
            float? delay = null,
            IProgress<float> progress = null,
            IProgress<float> smoothProgress = null,
            bool reuseExistingScene = true,
            CancellationToken cancellationToken = default
        )
            => await Instance.LoadSceneGroupAsync(
                groupIndex,
                delay,
                progress,
                smoothProgress,
                reuseExistingScene,
                cancellationToken
            );

        /// <summary>
        /// Asynchronously loads a scene group specified by its group name.
        /// </summary>
        /// <param name="groupName">The name of the scene group to load.</param>
        /// <param name="delay">Delay coefficient</param>
        /// <param name="progress">
        /// An IProgress object for reporting the loading progress,
        /// represented as a value between 0 and 1.
        /// </param>
        /// <param name="smoothProgress">
        /// An IProgress object for reporting the smoothed loading progress,
        /// represented as a value between 0 and 1.
        /// </param>
        /// <param name="reuseExistingScene">
        /// Indicating whether to reuse the existing scenes
        /// if it is already loaded, instead of reloading it.
        /// </param>
        /// <param name="cancellationToken">
        /// A CancellationToken that can be used to cancel the loading process.
        /// </param>
        /// <returns>A UniTask representing the asynchronous loading operation.</returns>
        public static async UniTask LoadAsync(
            string groupName,
            float? delay = null,
            IProgress<float> progress = null,
            IProgress<float> smoothProgress = null,
            bool reuseExistingScene = true,
            CancellationToken cancellationToken = default
        )
            => await Instance.LoadSceneGroupAsync(
                groupName,
                delay,
                progress,
                smoothProgress,
                reuseExistingScene,
                cancellationToken
            );
    }
}