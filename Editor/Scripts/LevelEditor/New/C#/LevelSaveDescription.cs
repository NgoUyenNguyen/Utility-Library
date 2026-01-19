namespace NgoUyenNguyen.Editor
{
    public class LevelSaveDescription<TLevelData>
    {
        public readonly TLevelData Data;
        public readonly string FolderPath;
        public readonly string Name;
        
        public LevelSaveDescription(TLevelData data, string name, string folderPath)
        {
            Data = data;
            FolderPath = folderPath;
            Name = name;
        }
    }

    public class LevelLoadDescription<TLevelData>
    {
        public readonly TLevelData Data;
        public readonly string FilePath;
        
        public LevelLoadDescription(TLevelData data, string filePath)
        {
            Data = data;
            FilePath = filePath;
        }
    }
}