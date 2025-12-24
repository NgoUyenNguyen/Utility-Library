#if UNITY_PIPELINE_URP || UNITY_PIPELINE_HDRP
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NgoUyenNguyen
{
    public class BlurEffectRenderFeature : ScriptableRendererFeature
    {
        private BlurEffectPass blurEffectPass;
        [SerializeField] private Material material;
        [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        public override void Create()
        {
            blurEffectPass = new BlurEffectPass
            {
                renderPassEvent = renderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (material == null) return;
            blurEffectPass.Setup(material);
            renderer.EnqueuePass(blurEffectPass);
        }
    }
}
#endif