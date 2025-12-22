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
            public TextureHandle Source;
            public TextureHandle Destination;
            public Material Material;
            public int GridSize;
            public float Spread;
        }

        public void Setup(Material material)
        {
            this.material = material;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var effectVolumeComponent = VolumeManager.instance.stack.GetComponent<Blur>();
            if (effectVolumeComponent == null || !effectVolumeComponent.IsActive() || material == null) return;

            var resourceData = frameData.Get<UniversalResourceData>();
            
            if (resourceData.isActiveTargetBackBuffer)
            {
                Debug.LogError("BlurEffectPass requires an intermediate texture.");
                return;
            }
            
            var cameraData = frameData.Get<UniversalCameraData>();

            var descriptor = cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;

            // Tạo 2 texture tạm
            var tmpTexture = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, 
                descriptor, 
                "_BlurTexH", 
                false);

            var gridSize = Mathf.CeilToInt(effectVolumeComponent.Strength.value * 6);
            gridSize = gridSize % 2 == 0 ? gridSize + 1 : gridSize;
            gridSize = Mathf.Clamp(gridSize, 1, 21);

            // Pass 1: Horizontal blur (source -> blurTextureH)
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Blur Pass Horizontal", out var passData))
            {
                passData.Source = resourceData.activeColorTexture;
                passData.Destination = tmpTexture;
                passData.Material = material;
                passData.GridSize = gridSize;
                passData.Spread = effectVolumeComponent.Strength.value;

                builder.UseTexture(passData.Source);
                builder.SetRenderAttachment(passData.Destination, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    data.Material.SetInteger("_GridSize", data.GridSize);
                    data.Material.SetFloat("_Spread", data.Spread);
                    Blitter.BlitTexture(context.cmd, data.Source, Vector2.one, data.Material, 0);
                });
            }

            // Pass 2: Vertical blur (blurTextureH -> blurTextureV)
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Blur Pass Vertical", out var passData))
            {
                passData.Source = tmpTexture;
                passData.Destination = resourceData.cameraColor;
                passData.Material = material;
                passData.GridSize = gridSize;
                passData.Spread = effectVolumeComponent.Strength.value;

                builder.UseTexture(passData.Source);
                builder.SetRenderAttachment(passData.Destination, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    data.Material.SetInteger("_GridSize", data.GridSize);
                    data.Material.SetFloat("_Spread", data.Spread);
                    Blitter.BlitTexture(context.cmd, data.Source, Vector2.one, data.Material, 1);
                });
            }
        }
    }
}