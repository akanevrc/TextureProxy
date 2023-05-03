﻿using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace akanevrc.TextureProxy
{
    [ScriptedImporter(1, "texproxy")]
    public class TextureProxyImporter : ScriptedImporter
    {
        public Color albedo = Color.white;

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

            var texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, 0, true);
            try
            {
                texture.LoadImage(bytes);
                var pixels = texture.GetPixels();
                for (var i = 0; i < pixels.Length; i++)
                {
                    pixels[i] *= albedo;
                }

                var output = TextureGenerator.GenerateTexture(
                    new TextureGenerationSettings(TextureImporterType.Default)
                    {
                        assetPath = ctx.assetPath,
                        enablePostProcessor = true,
                        platformSettings = new TextureImporterPlatformSettings()
                        {
                            format = TextureImporterFormat.Automatic,
                            maxTextureSize = 1024,
                            resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
                            textureCompression = TextureImporterCompression.Compressed,
                        },
                        sourceTextureInformation = new SourceTextureInformation()
                        {
                            width = 1024,
                            height = 1024,
                            containsAlpha = false,
                            hdr = false
                        },
                        textureImporterSettings = new TextureImporterSettings()
                        {
                            textureType = TextureImporterType.Default,
                            textureShape = TextureImporterShape.Texture2D,
                            sRGBTexture = true,
                            alphaSource = TextureImporterAlphaSource.FromInput,
                            alphaIsTransparency = false,
                            readable = false,
                            streamingMipmaps = false,
                            mipmapEnabled = true,
                            borderMipmap = false,
                            mipmapFilter = TextureImporterMipFilter.BoxFilter,
                            mipMapsPreserveCoverage = false,
                            fadeOut = false,
                            wrapMode = TextureWrapMode.Repeat,
                            filterMode = FilterMode.Bilinear,
                            aniso = 1
                        }
                    },
                    new NativeArray<Color32>(pixels.Select(color => (Color32)color).ToArray(), Allocator.Temp)
                );
                ctx.AddObjectToAsset("Texture", output.texture);
                ctx.SetMainObject(output.texture);
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }
}
