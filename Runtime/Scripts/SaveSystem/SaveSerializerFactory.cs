namespace NgoUyenNguyen
{
    public static class SaveSerializerFactory
    {
        public static ISaveSerializer Create()
        {
#if UNITY_EDITOR
            return new JsonSaveSerializer();
#else
            return new MessagePackSaveSerializer();
#endif
        }
    }
}