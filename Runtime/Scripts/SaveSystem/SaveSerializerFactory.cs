#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NgoUyenNguyen
{
    public static class SaveSerializerFactory
    {
        public static ISaveSerializer Create()
        {
#if UNITY_EDITOR
            var serializerOption = (SerializerOption)EditorPrefs.GetInt("SaveSerializerOption", (int)SerializerOption.Json);
            return serializerOption switch
            {
                SerializerOption.Json => new JsonSaveSerializer(),
                SerializerOption.MessagePack => new MessagePackSaveSerializer()
            };
#else
            return new MessagePackSaveSerializer();
#endif
        }
    }
    
    public enum SerializerOption
    {
        Json,
        MessagePack
    }
}