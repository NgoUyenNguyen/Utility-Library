using UnityEngine;
using UnityEngine.Rendering;

namespace NgoUyenNguyen
{
    [System.Serializable, VolumeComponentMenu("Post-processing Custom/Blur")]
    public class Blur : VolumeComponent, IPostProcessComponent
    {
        [SerializeField, Tooltip("Standard deviation (spread) of the blur. Grid size is approx. 3x larger.")]
        private ClampedFloatParameter strength = new (0f, 0f, 15f);

        public ClampedFloatParameter Strength => strength;

        public bool IsActive() => strength.value > 0f && active;
    }
}
