using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal
{
    public class CustomPostProcessFeature : ScriptableRendererFeature
    {
        public Shader bloomShader;
        private Material bloomMat;
        
        public CustomPostProcessPass customPass;
        
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(customPass);
        }
        
        public override void Create()
        {
            bloomMat = CoreUtils.CreateEngineMaterial(bloomShader);
            
            customPass = new CustomPostProcessPass(bloomMat);
        }
        
        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(bloomMat);
        }
    }
    
    public class CustomPostProcessPass : ScriptableRenderPass
    {
        RenderTextureDescriptor m_Descriptor;
        RenderTargetIdentifier m_Source;
        bool m_UseSwapBuffer;
        
        const int k_MaxPyramidSize = 16;
        int[] _BloomMipUp;
        int[] _BloomMipDown;
        
        private Material bloomMat;
        
        public CustomPostProcessPass(Material bloomMaterial)
        {
            bloomMat = bloomMaterial;
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            VolumeStack stack = VolumeManager.instance.stack;
            LongBloom bloomEffect = stack.GetComponent<LongBloom>();
            
            CommandBuffer cmd = CommandBufferPool.Get();
            
            ref CameraData cameraData = ref renderingData.cameraData;
            ref ScriptableRenderer renderer = ref cameraData.renderer;
            RenderTargetIdentifier source = m_UseSwapBuffer ? renderer.cameraColorTarget : m_Source;
            
            bool useRGBM;
            GraphicsFormat defaultHDR;
            if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Linear | FormatUsage.Render))
            {
                defaultHDR = GraphicsFormat.B10G11R11_UFloatPack32;
                useRGBM = false;
            }
            else
            {
                defaultHDR = QualitySettings.activeColorSpace == ColorSpace.Linear
                    ? GraphicsFormat.R8G8B8A8_SRGB
                : GraphicsFormat.R8G8B8A8_UNorm;
                useRGBM = true;
            }
            
            using (new ProfilingScope(cmd, new ProfilingSampler("Custom Post Process Pass")))
                SetupBloom(cmd, source, null, useRGBM, defaultHDR, bloomEffect, bloomMat);
        }
        
        void SetupBloom(CommandBuffer cmd, RenderTargetIdentifier source, Material uberMaterial,
                        bool m_UseRGBM, GraphicsFormat m_DefaultHDRFormat, LongBloom m_Bloom, Material bloomMaterial)
        {
            // Start at half-res
            int tw = m_Descriptor.width >> 1;
            int th = m_Descriptor.height >> 1;
            
            // Determine the iteration count
            int maxSize = Mathf.Max(tw, th);
            int iterations = Mathf.FloorToInt(Mathf.Log(maxSize, 2f) - 1);
            iterations -= m_Bloom.skipIterations.value;
            int mipCount = Mathf.Clamp(iterations, 1, k_MaxPyramidSize);
            
            // Pre-filtering parameters
            float clamp = m_Bloom.clamp.value;
            float threshold = Mathf.GammaToLinearSpace(m_Bloom.threshold.value);
            float thresholdKnee = threshold * 0.5f; // Hardcoded soft knee
            
            // Material setup
            float scatter = Mathf.Lerp(0.05f, 0.95f, m_Bloom.scatter.value);
            bloomMaterial.SetVector("_Params", new Vector4(scatter, clamp, threshold, thresholdKnee));
            CoreUtils.SetKeyword(bloomMaterial, ShaderKeywordStrings.BloomHQ, m_Bloom.highQualityFiltering.value);
            CoreUtils.SetKeyword(bloomMaterial, ShaderKeywordStrings.UseRGBM, m_UseRGBM);
            
            // Prefilter
            var desc = GetCompatibleDescriptor(tw, th, m_DefaultHDRFormat);
            cmd.GetTemporaryRT(_BloomMipDown[0], desc, FilterMode.Bilinear);
            cmd.GetTemporaryRT(_BloomMipUp[0], desc, FilterMode.Bilinear);
            Blit(cmd, source, _BloomMipDown[0], bloomMaterial, 0);
            
            // Downsample - gaussian pyramid
            int lastDown = _BloomMipDown[0];
            for (int i = 1; i < mipCount; i++)
            {
                tw = Mathf.Max(1, tw >> 1);
                th = Mathf.Max(1, th >> 1);
                int mipDown = _BloomMipDown[i];
                int mipUp = _BloomMipUp[i];
                
                desc.width = tw;
                desc.height = th;
                
                cmd.GetTemporaryRT(mipDown, desc, FilterMode.Bilinear);
                cmd.GetTemporaryRT(mipUp, desc, FilterMode.Bilinear);
                
                // Classic two pass gaussian blur - use mipUp as a temporary target
                //   First pass does 2x downsampling + 9-tap gaussian
                //   Second pass does 9-tap gaussian using a 5-tap filter + bilinear filtering
                Blit(cmd, lastDown, mipUp, bloomMaterial, 1);
                Blit(cmd, mipUp, mipDown, bloomMaterial, 2);
                
                lastDown = mipDown;
            }
            
            // Upsample (bilinear by default, HQ filtering does bicubic instead
            for (int i = mipCount - 2; i >= 0; i--)
            {
                int lowMip = (i == mipCount - 2) ? _BloomMipDown[i + 1] : _BloomMipUp[i + 1];
                int highMip = _BloomMipDown[i];
                int dst = _BloomMipUp[i];
                
                cmd.SetGlobalTexture("_SourceTexLowMip", lowMip);
                Blit(cmd, highMip, BlitDstDiscardContent(cmd, dst), bloomMaterial, 3);
            }
            
            // Cleanup
            for (int i = 0; i < mipCount; i++)
            {
                cmd.ReleaseTemporaryRT(_BloomMipDown[i]);
                if (i > 0) cmd.ReleaseTemporaryRT(_BloomMipUp[i]);
            }
            
            cmd.SetGlobalTexture("_Bloom_Texture", _BloomMipUp[0]);
            
#if false // NOTE(long): Do we need this
            // Setup bloom on uber
            var tint = m_Bloom.tint.value.linear;
            var luma = ColorUtils.Luminance(tint);
            tint = luma > 0f ? tint * (1f / luma) : Color.white;
            
            var bloomParams = new Vector4(m_Bloom.intensity.value, tint.r, tint.g, tint.b);
            uberMaterial.SetVector("_Bloom_Params", bloomParams);
            uberMaterial.SetFloat("_Bloom_RGBM", m_UseRGBM ? 1f : 0f);
            
            // Setup lens dirtiness on uber
            // Keep the aspect ratio correct & center the dirt texture, we don't want it to be
            // stretched or squashed
            var dirtTexture = m_Bloom.dirtTexture.value == null ? Texture2D.blackTexture : m_Bloom.dirtTexture.value;
            float dirtRatio = dirtTexture.width / (float)dirtTexture.height;
            float screenRatio = m_Descriptor.width / (float)m_Descriptor.height;
            var dirtScaleOffset = new Vector4(1f, 1f, 0f, 0f);
            float dirtIntensity = m_Bloom.dirtIntensity.value;
            
            if (dirtRatio > screenRatio)
            {
                dirtScaleOffset.x = screenRatio / dirtRatio;
                dirtScaleOffset.z = (1f - dirtScaleOffset.x) * 0.5f;
            }
            else if (screenRatio > dirtRatio)
            {
                dirtScaleOffset.y = dirtRatio / screenRatio;
                dirtScaleOffset.w = (1f - dirtScaleOffset.y) * 0.5f;
            }
            
            uberMaterial.SetVector("_LensDirt_Params", dirtScaleOffset);
            uberMaterial.SetFloat("_LensDirt_Intensity", dirtIntensity);
            uberMaterial.SetTexture("_LensDirt_Texture", dirtTexture);
            
            // Keyword setup - a bit convoluted as we're trying to save some variants in Uber...
            if (m_Bloom.highQualityFiltering.value)
                uberMaterial.EnableKeyword(dirtIntensity > 0f ? ShaderKeywordStrings.BloomHQDirt : ShaderKeywordStrings.BloomHQ);
            else
                uberMaterial.EnableKeyword(dirtIntensity > 0f ? ShaderKeywordStrings.BloomLQDirt : ShaderKeywordStrings.BloomLQ);
#endif
        }
        
        private BuiltinRenderTextureType BlitDstDiscardContent(CommandBuffer cmd, RenderTargetIdentifier rt)
        {
            // We set depth to DontCare because rt might be the source of PostProcessing used as a temporary target
            // Source typically comes with a depth buffer and right now we don't have a way to only bind the color attachment of a RenderTargetIdentifier
            cmd.SetRenderTarget(new RenderTargetIdentifier(rt, 0, CubemapFace.Unknown, -1),
                                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            return BuiltinRenderTextureType.CurrentActive;
        }
        
        RenderTextureDescriptor GetCompatibleDescriptor(int width, int height, GraphicsFormat format, int depthBufferBits = 0)
        {
            var desc = m_Descriptor;
            desc.depthBufferBits = depthBufferBits;
            desc.msaaSamples = 1;
            desc.width = width;
            desc.height = height;
            desc.graphicsFormat = format;
            return desc;
        }
    }
}