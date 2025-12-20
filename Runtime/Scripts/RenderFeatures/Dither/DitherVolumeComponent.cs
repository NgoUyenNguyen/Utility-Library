using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NgoUyenNguyen
{
    [VolumeComponentMenu("Post-processing Custom/Dither")]
    [SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
    public sealed class DitherVolumeComponent : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Enter the description for the property that is shown when hovered")]
        public ClampedFloatParameter intensity = new (0f, 0f, 1f, true);

        public bool IsActive() => intensity.value > 0f;
    }
}
