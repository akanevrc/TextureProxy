using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public FilterMode filterMode;
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
        public TextureImporterFormat format;
        public int maxTextureSize;
        public TextureResizeAlgorithm resizeAlgorithm;
        public TextureImporterCompression textureCompression;

        public static explicit operator TextureImporterPlatformSettings(TextureProxyImporterPlatformSettings settings)
        {
            return new TextureImporterPlatformSettings()
            {
                format = settings.format,
                maxTextureSize = settings.maxTextureSize,
                resizeAlgorithm = settings.resizeAlgorithm,
                textureCompression = settings.textureCompression
            };
        }
    }

    [ScriptedImporter(1, "texproxy")]
    public class TextureProxyImporter : ScriptedImporter
    {
        public List<PixelFilterSettings> pixelFilterSettingsList = new List<PixelFilterSettings>();
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
                readable = false,
                streamingMipmaps = false,
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
                filterMode = FilterMode.Bilinear,
                aniso = 1
            };
        public TextureProxyImporterPlatformSettings textureImporterPlatformSettings =
            new TextureProxyImporterPlatformSettings()
            {
                format = TextureImporterFormat.Automatic,
                maxTextureSize = 1024,
                resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
                textureCompression = TextureImporterCompression.Compressed
            };

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var bytes = (byte[])null;
            try
            {
                bytes = File.ReadAllBytes(ctx.assetPath);
            }
            catch (IOException)
            {
                return;
            }

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
                texture.LoadImage(bytes);
                var pixels = PixelFilter.FilterAll(this.pixelFilterSettingsList, texture.GetPixels());

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
                    new NativeArray<Color32>(pixels.Select(color => (Color32)color).ToArray(), Allocator.Temp)
                );
                ctx.AddObjectToAsset("Texture", output.texture);
                ctx.SetMainObject(output.texture);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
