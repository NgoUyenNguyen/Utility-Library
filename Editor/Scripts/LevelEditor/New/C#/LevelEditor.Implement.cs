using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace NgoUyenNguyen.Editor
{
    public abstract partial class LevelEditorWindow<TLevelData>
    {
        protected abstract void Save(LevelSaveDescription<TLevelData> levelSaveDescription);

        protected abstract LevelLoadDescription<TLevelData> Load(AssetReference levelReference);

        protected abstract TLevelData New();
        
        protected virtual VisualElement CreateContent()
        {
            return null;
        }
        
        protected virtual void OnDrawIMGUIContent()
        {
        }

        protected virtual void OnDrawIMGUIToolbarLeft()
        {
        }
        
        protected virtual void OnDrawIMGUIToolbarRight()
        {
        }
        
        protected virtual VisualElement AppendLeftToolbar()
        {
            return null;
        }
        
        protected virtual VisualElement AppendRightToolbar()
        {
            return null;
        }
    }
}