#if URP

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NgoUyenNguyen
{
    [VolumeComponentMenu("Post-processing Custom/Dither")]
    [SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
    public sealed class Dither : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Enter the description for the property that is shown when hovered")]
        public ClampedFloatParameter intensity = new (0f, 0f, 1f, true);
        public ClampedFloatParameter threshold = new (0f, -1f, 1f, true);
        public ClampedFloatParameter scale = new (.25f, 1e-3f, 1f, true);
        public ColorParameter colorA = new (Color.black);
        public ColorParameter colorB = new (Color.white);

        public bool IsActive() => intensity.value > 0f;
    }
}
#endif
