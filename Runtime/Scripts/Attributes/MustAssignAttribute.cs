using System;
using UnityEngine;

namespace NgoUyenNguyen
{
    /// <summary>
    /// Ensure that the field is assigned. If not, an icon will be displayed in the hierarchy
    /// to show that the GameObject has an unassigned field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class MustAssignAttribute : PropertyAttribute
    {
        
    }
}
