namespace NgoUyenNguyen.Editor
{
    /// <summary>
    /// Represents a description for saving level data, including the data itself, name, and folder path.
    /// </summary>
    /// <typeparam name="TLevelData">
    /// The type of the level data associated with the save operation.
    /// </typeparam>
    public class LevelSaveDescription<TLevelData>
    {
        /// <summary>
        /// Represents the level data associated with the save operation.
        /// </summary>
        /// <typeparam name="TLevelData">
        /// The type of the level data.
        /// </typeparam>
        public readonly TLevelData Data;

        /// <summary>
        /// Specifies the folder path where the level data will be saved.
        /// </summary>
        public readonly string FolderPath;

        /// <summary>
        /// Represents the name of the level associated with a save operation.
        /// </summary>
        public readonly string Name;

        public LevelSaveDescription(TLevelData data, string name, string folderPath)
        {
            Data = data;
            FolderPath = folderPath;
            Name = name;
        }
    }

    /// <summary>
    /// Represents a description for loading level data, including the data itself and the file path from which it was loaded.
    /// </summary>
    /// <typeparam name="TLevelData">
    /// The type of the level data associated with the load operation.
    /// </typeparam>
    public class LevelLoadDescription<TLevelData>
    {
        /// <summary>
        /// Represents the data associated with the level load operation.
        /// </summary>
        /// <typeparam name="TLevelData">
        /// The type of the data for the level being loaded.
        /// </typeparam>
        public readonly TLevelData Data;

        /// <summary>
        /// Represents the file path associated with the level load operation.
        /// </summary>
        public readonly string FilePath;

        public LevelLoadDescription(TLevelData data, string filePath)
        {
            Data = data;
            FilePath = filePath;
        }
    }
}