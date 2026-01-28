using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace NgoUyenNguyen.Editor
{
    public abstract partial class LevelEditorWindow<TLevelData>
    {
        /// <summary>
        /// Saves level data and associated information.
        /// </summary>
        /// <param name="levelSaveDescription">
        /// A description of the level being saved, including its data, name, and folder path.
        /// </param>
        protected abstract void Save(LevelSaveDescription<TLevelData> levelSaveDescription);

        /// <summary>
        /// Loads level data and associated file information.
        /// </summary>
        /// <param name="levelReference">
        /// A reference to the level asset to be loaded. Null if not loaded from <see cref="NgoUyenNguyen.ScriptableObjects.LevelReference"/>.
        /// </param>
        /// <returns>
        /// A description of the loaded level, including its data and file path.
        /// </returns>
        protected abstract LevelLoadDescription<TLevelData> Load(AssetReference levelReference);

        /// <summary>
        /// Creates a new instance of the level data managed by the editor.
        /// </summary>
        /// <returns>
        /// A new instance of the type <typeparamref name="TLevelData"/> representing the level's data structure.
        /// </returns>
        protected abstract TLevelData New();

        /// <summary>
        /// Creates and returns the main visual content of the level editor window.
        /// </summary>
        /// <returns>
        /// A <see cref="VisualElement"/> representing the primary UI content of the editor window.
        /// </returns>
        protected virtual VisualElement CreateContent()
        {
            return null;
        }

        /// <summary>
        /// Renders the IMGUI content section of the level editor window.
        /// This method is called during the IMGUI rendering phase to draw custom editor UI elements.
        /// </summary>
        protected virtual void OnDrawIMGUIContent()
        {
        }

        /// <summary>
        /// Renders additional IMGUI-based controls on the left side of the toolbar in the level editor window.
        /// </summary>
        protected virtual void OnDrawIMGUIToolbarLeft()
        {
        }

        /// <summary>
        /// Renders additional IMGUI-based controls on the right side of the toolbar in the level editor window.
        /// </summary>
        protected virtual void OnDrawIMGUIToolbarRight()
        {
        }

        /// <summary>
        /// Appends a toolbar to the left side of the editor window's UI.
        /// </summary>
        /// <returns>
        /// A VisualElement representing the appended left toolbar.
        /// </returns>
        protected virtual VisualElement AppendLeftToolbar()
        {
            return null;
        }

        /// <summary>
        /// Appends a toolbar to the right side of the editor window's UI.
        /// </summary>
        /// <returns>
        /// A VisualElement representing the appended right toolbar.
        /// </returns>
        protected virtual VisualElement AppendRightToolbar()
        {
            return null;
        }
    }
}