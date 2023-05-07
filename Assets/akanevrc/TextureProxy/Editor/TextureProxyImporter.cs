using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace akanevrc.TextureProxy
{
    [Serializable]
    public struct SourceTextureProxyInformation
    {
        public int width;
        public int height;
        public bool containsAlpha;
        public bool hdr;

        public static explicit operator SourceTextureInformation(SourceTextureProxyInformation information)
        {
            return new SourceTextureInformation()
            {
                width = information.width,
                height = information.height,
                containsAlpha = information.containsAlpha,
                hdr = information.hdr
            };
        }
    }

    [Serializable]
    public struct TextureProxyImporterSettings
    {
        public TextureImporterType textureType;
        public TextureImporterShape textureShape;
        public TextureImporterGenerateCubemap generateCubemap;
        public TextureImporterCubemapConvolution cubemapConvolution;
        public bool seamlessCubemap;
        public bool sRGBTexture;
        public TextureImporterAlphaSource alphaSource;
        public bool alphaIsTransparency;
        public TextureImporterNPOTScale npotScale;
        public bool readable;
        public bool streamingMipmaps;
        public int streamingMipmapsPriority;
        public bool mipmapEnabled;
        public bool borderMipmap;
        public TextureImporterMipFilter mipmapFilter;
        public bool mipMapsPreserveCoverage;
        public float alphaTestReferenceValue;
        public bool fadeOut;
        public int mipmapFadeDistanceStart;
        public int mipmapFadeDistanceEnd;
        public TextureWrapMode wrapMode;
        public UnityEngine.FilterMode filterMode;
        public int aniso;

        public static explicit operator TextureImporterSettings(TextureProxyImporterSettings settings)
        {
            return new TextureImporterSettings
            {
                textureType = settings.textureType,
                textureShape = settings.textureShape,
                generateCubemap = settings.generateCubemap,
                cubemapConvolution = settings.cubemapConvolution,
                seamlessCubemap = settings.seamlessCubemap,
                sRGBTexture = settings.sRGBTexture,
                alphaSource = settings.alphaSource,
                alphaIsTransparency = settings.alphaIsTransparency,
                npotScale = settings.npotScale,
                readable = settings.readable,
                streamingMipmaps = settings.streamingMipmaps,
                streamingMipmapsPriority = settings.streamingMipmapsPriority,
                mipmapEnabled = settings.mipmapEnabled,
                borderMipmap = settings.borderMipmap,
                mipmapFilter = settings.mipmapFilter,
                mipMapsPreserveCoverage = settings.mipMapsPreserveCoverage,
                alphaTestReferenceValue = settings.alphaTestReferenceValue,
                fadeOut = settings.fadeOut,
                mipmapFadeDistanceStart = settings.mipmapFadeDistanceStart,
                mipmapFadeDistanceEnd = settings.mipmapFadeDistanceEnd,
                wrapMode = settings.wrapMode,
                filterMode = settings.filterMode,
                aniso = settings.aniso
            };
        }
    }

    [Serializable]
    public struct TextureProxyImporterPlatformSettings
    {
        public int maxTextureSize;
        public TextureResizeAlgorithm resizeAlgorithm;
        public TextureImporterFormat format;
        public TextureImporterCompression textureCompression;
        public bool crunchedCompression;

        public static explicit operator TextureImporterPlatformSettings(TextureProxyImporterPlatformSettings settings)
        {
            return new TextureImporterPlatformSettings()
            {
                maxTextureSize = settings.maxTextureSize,
                resizeAlgorithm = settings.resizeAlgorithm,
                format = settings.format,
                textureCompression = settings.textureCompression,
                crunchedCompression = settings.crunchedCompression
            };
        }
    }

    [ScriptedImporter(1, "texproxy")]
    public class TextureProxyImporter : ScriptedImporter
    {
        internal static Texture activeTexture;
        internal static TextureImporter activeImporter;

        public List<FilterSettings> filterSettingsList = new List<FilterSettings>();
        public SourceTextureProxyInformation sourceTextureInformation =
            new SourceTextureProxyInformation()
            {
                width = 1024,
                height = 1024,
                containsAlpha = true,
                hdr = false
            };
        public TextureProxyImporterSettings textureImporterSettings =
            new TextureProxyImporterSettings()
            {
                textureType = TextureImporterType.Default,
                textureShape = TextureImporterShape.Texture2D,
                generateCubemap = TextureImporterGenerateCubemap.AutoCubemap,
                cubemapConvolution = TextureImporterCubemapConvolution.None,
                seamlessCubemap = false,
                sRGBTexture = true,
                alphaSource = TextureImporterAlphaSource.FromInput,
                alphaIsTransparency = false,
                npotScale = TextureImporterNPOTScale.ToNearest,
                readable = false,
                streamingMipmaps = true,
                streamingMipmapsPriority = 0,
                mipmapEnabled = true,
                borderMipmap = false,
                mipmapFilter = TextureImporterMipFilter.BoxFilter,
                mipMapsPreserveCoverage = false,
                alphaTestReferenceValue = 0.5F,
                fadeOut = false,
                mipmapFadeDistanceStart = 1,
                mipmapFadeDistanceEnd = 3,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = UnityEngine.FilterMode.Bilinear,
                aniso = 1
            };
        public TextureProxyImporterPlatformSettings textureImporterPlatformSettings =
            new TextureProxyImporterPlatformSettings()
            {
                maxTextureSize = 2048,
                resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
                format = TextureImporterFormat.Automatic,
                textureCompression = TextureImporterCompression.Compressed,
                crunchedCompression = false
            };

        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (TextureProxyImporter.activeTexture != null && TextureProxyImporter.activeImporter != null)
            {
                ExtractSettings(TextureProxyImporter.activeTexture, TextureProxyImporter.activeImporter);
                TextureProxyImporter.activeTexture = null;
                TextureProxyImporter.activeImporter = null;
            }

            var bytes = (byte[])null;
            try
            {
                bytes = File.ReadAllBytes(ctx.assetPath);
            }
            catch (IOException)
            {
                return;
            }

            var renderTexture0 = RenderTexture.GetTemporary
            (
                this.sourceTextureInformation.width,
                this.sourceTextureInformation.height,
                0
            );
            var renderTexture1 = RenderTexture.GetTemporary
            (
                this.sourceTextureInformation.width,
                this.sourceTextureInformation.height,
                0
            );

            var texture =
                new Texture2D
                (
                    this.sourceTextureInformation.width,
                    this.sourceTextureInformation.height,
                    TextureFormat.RGBA32,
                    0,
                    true
                );
            try
            {
                ApplyFilters(bytes, texture, renderTexture0, renderTexture1);

                var output = TextureGenerator.GenerateTexture
                (
                    new TextureGenerationSettings(textureImporterSettings.textureType)
                    {
                        assetPath = ctx.assetPath,
                        enablePostProcessor = false,
                        platformSettings = (TextureImporterPlatformSettings)textureImporterPlatformSettings,
                        sourceTextureInformation = (SourceTextureInformation)sourceTextureInformation,
                        textureImporterSettings = (TextureImporterSettings)textureImporterSettings
                    },
                    new NativeArray<Color32>(texture.GetPixels32(), Allocator.Temp)
                );
                ctx.AddObjectToAsset("Texture", output.texture);
                ctx.SetMainObject(output.texture);
            }
            finally
            {
                RenderTexture.ReleaseTemporary(renderTexture0);
                RenderTexture.ReleaseTemporary(renderTexture1);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private void ApplyFilters(byte[] bytes, Texture2D source, RenderTexture renderTexture0, RenderTexture renderTexture1)
        {
            source.LoadImage(bytes);
            var renderTexture = Blitter.Blit(this.filterSettingsList, source, renderTexture0, renderTexture1);

            var oldRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            source.ReadPixels(new Rect(0F, 0F, renderTexture.width, renderTexture.height), 0, 0);
            source.Apply();
            RenderTexture.active = oldRenderTexture;
        }

        public static bool SupportSettings(TextureImporter importer, out string[] errors)
        {
            var errorList = new List<string>();

            if (importer.textureType != TextureImporterType.Default)
            {
                errorList.Add("Texture type of TextureImporter must be 'Default'.");
            }

            if (importer.textureShape != TextureImporterShape.Texture2D)
            {
                errorList.Add("Texture shape of TextureImporter must be 'Texture2D'.");
            }

            if (importer.GetPlatformTextureSettings("Standalone").overridden || importer.GetPlatformTextureSettings("Android").overridden)
            {
                errorList.Add("Default platform texture settings must not be overridden.");
            }

            errors = errorList.ToArray();
            return errors.Length == 0;
        }

        public void ExtractSettings(Texture texture, TextureImporter importer)
        {
            this.sourceTextureInformation =
                new SourceTextureProxyInformation()
                {
                    width = texture.width,
                    height = texture.height,
                    containsAlpha = importer.DoesSourceTextureHaveAlpha(),
                    hdr = false
                };
            this.textureImporterSettings =
                new TextureProxyImporterSettings()
                {
                    textureType = importer.textureType,
                    textureShape = importer.textureShape,
                    generateCubemap = importer.generateCubemap,
                    cubemapConvolution = TextureImporterCubemapConvolution.None,
                    seamlessCubemap = false,
                    sRGBTexture = importer.sRGBTexture,
                    alphaSource = importer.alphaSource,
                    alphaIsTransparency = importer.alphaIsTransparency,
                    npotScale = importer.npotScale,
                    readable = importer.isReadable,
                    streamingMipmaps = importer.streamingMipmaps || importer.mipmapEnabled,
                    streamingMipmapsPriority = importer.streamingMipmapsPriority,
                    mipmapEnabled = importer.mipmapEnabled,
                    borderMipmap = importer.borderMipmap,
                    mipmapFilter = importer.mipmapFilter,
                    mipMapsPreserveCoverage = importer.mipMapsPreserveCoverage,
                    alphaTestReferenceValue = importer.alphaTestReferenceValue,
                    fadeOut = importer.fadeout,
                    mipmapFadeDistanceStart = importer.mipmapFadeDistanceStart,
                    mipmapFadeDistanceEnd = importer.mipmapFadeDistanceEnd,
                    wrapMode = importer.wrapMode,
                    filterMode = importer.filterMode,
                    aniso = importer.anisoLevel
                };
            var platformSettings = importer.GetDefaultPlatformTextureSettings();
            this.textureImporterPlatformSettings =
                new TextureProxyImporterPlatformSettings()
                {
                    maxTextureSize = platformSettings.maxTextureSize,
                    resizeAlgorithm = platformSettings.resizeAlgorithm,
                    format = platformSettings.format,
                    textureCompression = platformSettings.textureCompression,
                    crunchedCompression = platformSettings.crunchedCompression
                };
        }
    }
}
