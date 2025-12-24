#if URP

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace NgoUyenNguyen
{
    public class DitherEffectRenderFeature : ScriptableRendererFeature
    {
        class DitherEffectPass : ScriptableRenderPass
        {
            class PassData
            {
                public TextureHandle Source;
                public TextureHandle Destination;
                public Material Material;
                public float Intensity;
                public float Threshold;
                public float Scale;
                public Color ColorA;
                public Color ColorB;
            }

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
                var customEffect = VolumeManager.instance.stack.GetComponent<Dither>();
                if (customEffect == null || !customEffect.IsActive()) return;

                var resourceData = frameData.Get<UniversalResourceData>();
                if (resourceData.isActiveTargetBackBuffer)
                {
                    Debug.LogError(
                        $"Skipping render pass. DitherEffectPass requires an intermediate ColorTexture, can not use the BackBuffer as a texture input.");
                    return;
                }

                var cameraData = frameData.Get<UniversalCameraData>();
                var descriptor = cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 0;
                descriptor.msaaSamples = 1;

                var destination = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    descriptor,
                    "_TemporaryTexture",
                    false);

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Dither Effect Pass", out var passData))
                {
                    passData.Source = resourceData.activeColorTexture;
                    passData.Destination = destination;
                    passData.Material = blitMaterial;
                    passData.Intensity = customEffect.intensity.value;
                    passData.Threshold = customEffect.threshold.value;
                    passData.Scale = customEffect.scale.value;
                    passData.ColorA = customEffect.colorA.value;
                    passData.ColorB = customEffect.colorB.value;

                    builder.UseTexture(passData.Source);
                    builder.SetRenderAttachment(passData.Destination, 0);

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        data.Material.SetFloat("_Step_Threshold", data.Threshold);
                        data.Material.SetFloat("_Dither_Scale", data.Scale);
                        data.Material.SetFloat("_Intensity", data.Intensity);
                        data.Material.SetColor("_Color_A", data.ColorA);
                        data.Material.SetColor("_Color_B", data.ColorB);
                        Blitter.BlitTexture(context.cmd, data.Source, Vector2.one, data.Material, 0);
                    });
                }

                resourceData.cameraColor = destination;
            }
        }

        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Material material;

        DitherEffectPass m_ScriptablePass;

        /// <inheritdoc/>
        public override void Create()
        {
            m_ScriptablePass = new DitherEffectPass
            {
                // Configures where the render pass should be injected.
                renderPassEvent = renderPassEvent
            };
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
#endif