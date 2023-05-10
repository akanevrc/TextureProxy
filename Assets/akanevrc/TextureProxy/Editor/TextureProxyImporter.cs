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
        public int compressionQuality;

        public static explicit operator TextureImporterPlatformSettings(TextureProxyImporterPlatformSettings settings)
        {
            return new TextureImporterPlatformSettings()
            {
                maxTextureSize = settings.maxTextureSize,
                resizeAlgorithm = settings.resizeAlgorithm,
                format = settings.format,
                textureCompression = settings.textureCompression,
                crunchedCompression = settings.crunchedCompression,
                compressionQuality = settings.compressionQuality
            };
        }
    }

    [ScriptedImporter(1, "texproxy")]
    public class TextureProxyImporter : ScriptedImporter
    {
        public static readonly string workFolder = "Assets/akanevrc/TextureProxy/Editor/work";

        internal static Texture activeTexture;
        internal static TextureImporter activeImporter;
        internal static bool workFileCreated;

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
                crunchedCompression = false,
                compressionQuality = 50
            };

        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (TextureProxyImporter.activeTexture != null && TextureProxyImporter.activeImporter != null)
            {
                ExtractSettings(TextureProxyImporter.activeTexture, TextureProxyImporter.activeImporter);
                TextureProxyImporter.activeTexture = null;
                TextureProxyImporter.activeImporter = null;
            }

            var pixels = LoadAndApply(ctx.assetPath);
            if (pixels == null) return;

            var output =
                TextureGenerator.GenerateTexture
                (
                    new TextureGenerationSettings(textureImporterSettings.textureType)
                    {
                        assetPath = ctx.assetPath,
                        enablePostProcessor = false,
                        platformSettings = (TextureImporterPlatformSettings)textureImporterPlatformSettings,
                        sourceTextureInformation = (SourceTextureInformation)sourceTextureInformation,
                        textureImporterSettings = (TextureImporterSettings)textureImporterSettings
                    },
                    new NativeArray<Color32>(pixels, Allocator.Temp)
                );
            ctx.AddObjectToAsset("Texture", output.texture);
            ctx.SetMainObject(output.texture);
        }

        private Color32[] LoadAndApply(string assetPath)
        {
            var ext = Path.GetExtension(Path.GetFileNameWithoutExtension(assetPath)).ToLower();
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
            {
                return LoadAndApplyAsPngOrJpeg(assetPath);
            }
            else
            {
                return LoadAndApplyAsGenericImage(assetPath);
            }
        }

        private Color32[] LoadAndApplyAsPngOrJpeg(string assetPath)
        {
            var bytes = (byte[])null;
            try
            {
                bytes = File.ReadAllBytes(assetPath);
            }
            catch (IOException)
            {
                return null;
            }

            var w = Mathf.NextPowerOfTwo(Mathf.Min(this.sourceTextureInformation.width, this.textureImporterPlatformSettings.maxTextureSize));
            var h = Mathf.NextPowerOfTwo(Mathf.Min(this.sourceTextureInformation.height, this.textureImporterPlatformSettings.maxTextureSize));

            var renderTexture0 = RenderTexture.GetTemporary(w, h, 0);
            var renderTexture1 = RenderTexture.GetTemporary(w, h, 0);

            var src =
                new Texture2D
                (
                    w,
                    h,
                    this.sourceTextureInformation.containsAlpha ? TextureFormat.ARGB32 : TextureFormat.RGB24,
                    0,
                    true
                );
            var dest =
                new Texture2D
                (
                    w,
                    h,
                    this.sourceTextureInformation.containsAlpha ? TextureFormat.ARGB32 : TextureFormat.RGB24,
                    0,
                    true
                );
            try
            {
                src.LoadImage(bytes);
                ApplyFilters(src, dest, renderTexture0, renderTexture1);
                return dest.GetPixels32();
            }
            finally
            {
                RenderTexture.ReleaseTemporary(renderTexture0);
                RenderTexture.ReleaseTemporary(renderTexture1);
                UnityEngine.Object.DestroyImmediate(src);
                UnityEngine.Object.DestroyImmediate(dest);
            }
        }

        private Color32[] LoadAndApplyAsGenericImage(string assetPath)
        {
            if (!assetPath.EndsWith(".texproxy")) return null;

            var workAssetPath = Path.Combine(TextureProxyImporter.workFolder, Path.GetFileNameWithoutExtension(assetPath));
            
            if (workFileCreated)
            {
                workFileCreated = false;
            }
            else
            {
                AssetDatabase.DeleteAsset(workAssetPath);
                File.Copy(assetPath, workAssetPath, true);
                AssetDatabase.Refresh();
            }

            var workTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(workAssetPath);

            var w = Mathf.NextPowerOfTwo(Mathf.Min(this.sourceTextureInformation.width, this.textureImporterPlatformSettings.maxTextureSize));
            var h = Mathf.NextPowerOfTwo(Mathf.Min(this.sourceTextureInformation.height, this.textureImporterPlatformSettings.maxTextureSize));

            var renderTexture0 = RenderTexture.GetTemporary(w, h, 0);
            var renderTexture1 = RenderTexture.GetTemporary(w, h, 0);

            var dest =
                new Texture2D
                (
                    w,
                    h,
                    this.sourceTextureInformation.containsAlpha ? TextureFormat.ARGB32 : TextureFormat.RGB24,
                    0,
                    true
                );
            try
            {
                ApplyFilters(workTexture, dest, renderTexture0, renderTexture1);
                return dest.GetPixels32();
            }
            finally
            {
                RenderTexture.ReleaseTemporary(renderTexture0);
                RenderTexture.ReleaseTemporary(renderTexture1);
                UnityEngine.Object.DestroyImmediate(dest);
                AssetDatabase.DeleteAsset(workAssetPath);
                AssetDatabase.Refresh();
            }
        }

        private void ApplyFilters(Texture2D src, Texture2D dest, RenderTexture renderTexture0, RenderTexture renderTexture1)
        {
            var renderTexture = Blitter.Blit(this.filterSettingsList, src, renderTexture0, renderTexture1);

            var oldRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            dest.ReadPixels(new Rect(0F, 0F, renderTexture.width, renderTexture.height), 0, 0);
            dest.Apply();
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
                    crunchedCompression = platformSettings.crunchedCompression,
                    compressionQuality = platformSettings.compressionQuality
                };
        }
    }
}
