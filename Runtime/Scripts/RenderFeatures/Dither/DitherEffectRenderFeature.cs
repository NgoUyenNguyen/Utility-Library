using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace NgoUyenNguyen
{
    public class DitherEffectRenderFeature : ScriptableRendererFeature
    {
        class DitherEffectPass : ScriptableRenderPass
        {
            private const string PassName = "DitherEffectPass";
            private Material blitMaterial;

            public void Setup(Material material)
            {
                blitMaterial = material;
                requiresIntermediateTexture = true;
            }

            // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
            // FrameData is a context container through which URP resources can be accessed and managed.
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var customEffect = VolumeManager.instance.stack.GetComponent<DitherVolumeComponent>();
                if (customEffect == null || !customEffect.IsActive()) return;
                
                var resourceData = frameData.Get<UniversalResourceData>();
                if (resourceData.isActiveTargetBackBuffer)
                {
                    Debug.LogError($"Skipping render pass. DitherEffectPass requires an intermediate ColorTexture, can not use the BackBuffer as a texture input.");
                    return;
                }

                var source = resourceData.activeColorTexture;
                var destinationDesc = renderGraph.GetTextureDesc(source);
                destinationDesc.name = $"CameraColor-{PassName}";
                destinationDesc.clearBuffer = false;
                
                var destination = renderGraph.CreateTexture(destinationDesc);
                var para = new RenderGraphUtils.BlitMaterialParameters(source, destination, blitMaterial, 0);
                renderGraph.AddBlitPass(para, PassName);
                
                resourceData.cameraColor = destination;
            }
        }

        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Material material;

        DitherEffectPass m_ScriptablePass;

        /// <inheritdoc/>
        public override void Create()
        {
            m_ScriptablePass = new DitherEffectPass();

            // Configures where the render pass should be injected.
            m_ScriptablePass.renderPassEvent = renderPassEvent;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (material == null) return;
            m_ScriptablePass.Setup(material);
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}