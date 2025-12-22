using UnityEngine;

#if UNITY_EDITOR
namespace NgoUyenNguyen
{
    public struct JsonSaveSerializer : ISaveSerializer
    {
        public string FileExtension => ".json";

        public byte[] Serialize<T>(T data)
        {
            var json = JsonUtility.ToJson(data, true);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonUtility.FromJson<T>(json);
        }
    }
}
#endif