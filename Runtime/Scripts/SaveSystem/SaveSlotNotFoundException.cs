using System;

namespace NgoUyenNguyen
{
    public sealed class SaveSlotNotFoundException : Exception
    {
        public SaveSlotNotFoundException(string slot)
            : base($"Save slot '{slot}' not found.") { }
    }
}