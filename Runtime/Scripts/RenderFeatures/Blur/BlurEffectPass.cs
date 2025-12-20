using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace NgoUyenNguyen
{
    public class BlurEffectPass : ScriptableRenderPass
    {
        private Material material;

        private class PassData
        {
            public TextureHandle source;
            public TextureHandle destination;
            public Material material;
            public int gridSize;
            public float spread;
        }

        public void Setup(Material material)
        {
            this.material = material;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var effectVolumeComponent = VolumeManager.instance.stack.GetComponent<BlurEffectVolumeComponent>();
            if (effectVolumeComponent == null || !effectVolumeComponent.IsActive() || material == null) return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            var descriptor = cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            var blurTexture = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, 
                descriptor, 
                "_BlurTex", 
                false);

            var gridSize = Mathf.CeilToInt(effectVolumeComponent.Strength.value * 6);
            gridSize = gridSize % 2 == 0 ? gridSize + 1 : gridSize;

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Blur Pass 1", out var passData))
            {
                passData.source = resourceData.activeColorTexture;
                passData.destination = blurTexture;
                passData.material = material;
                passData.gridSize = gridSize;
                passData.spread = effectVolumeComponent.Strength.value;

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
                passData.spread = effectVolumeComponent.Strength.value;

                builder.UseTexture(passData.source);
                builder.SetRenderAttachment(passData.destination, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    data.material.SetInteger("_GridSize", data.gridSize);
                    data.material.SetFloat("_Spread", data.spread);
                    Blitter.BlitTexture(context.cmd, data.source, Vector2.one, data.material, 1);
                });
            }
        }
    }
}