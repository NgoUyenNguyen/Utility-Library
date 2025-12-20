using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace NgoUyenNguyen
{
    public class BlurRenderPass : ScriptableRenderPass
    {
        private Material material;
        private BlurSettings settings;

        private class PassData
        {
            public TextureHandle source;
            public TextureHandle destination;
            public Material material;
            public int gridSize;
            public float spread;
        }

        public bool Setup()
        {
            settings = VolumeManager.instance.stack.GetComponent<BlurSettings>();
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

            if (settings == null || !settings.IsActive()) return false;

            if (material == null)
            {
                material = new Material(UnityEngine.Shader.Find("PostProcessing/Blur"));
            }
            return material != null;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (settings == null || !settings.IsActive() || material == null) return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            var descriptor = cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            var blurTexture = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, 
                descriptor, 
                "_BlurTex", 
                false);

            var gridSize = Mathf.CeilToInt(settings.Strength.value * 6);
            gridSize = gridSize % 2 == 0 ? gridSize + 1 : gridSize;

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Blur Pass 1", out var passData))
            {
                passData.source = resourceData.activeColorTexture;
                passData.destination = blurTexture;
                passData.material = material;
                passData.gridSize = gridSize;
                passData.spread = settings.Strength.value;

                builder.UseTexture(passData.source);
                builder.SetRenderAttachment(passData.destination, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    data.material.SetInteger("_GridSize", data.gridSize);
                    data.material.SetFloat("_Spread", data.spread);
                    Blitter.BlitTexture(context.cmd, data.source, Vector2.one, data.material, 0);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Blur Pass 2", out var passData))
            {
                passData.source = blurTexture;
                passData.destination = resourceData.activeColorTexture;
                passData.material = material;
                passData.gridSize = gridSize;
                passData.spread = settings.Strength.value;

                builder.UseTexture(passData.source);
                builder.SetRenderAttachment(passData.destination, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    data.material.SetInteger("_GridSize", data.gridSize);
                    data.material.SetFloat("_Spread", data.spread);
                    Blitter.BlitTexture(context.cmd, data.source, Vector2.one, data.material, 0);
                });
            }
        }
    }
}