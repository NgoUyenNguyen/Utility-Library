using MessagePack;

namespace NgoUyenNguyen
{
    public struct MessagePackSaveSerializer : ISaveSerializer
    {
        public string FileExtension => ".sav";

        public byte[] Serialize<T>(T data)
        {
            return MessagePackSerializer.Serialize(data);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }
    }
}