using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace akanevrc.TextureProxy
{
    public static class Blitter
    {
        public static readonly string filterMaterialPath = "Assets/akanevrc/TextureProxy/Editor/Shaders/Filter.mat";

        public static RenderTexture Blit(IEnumerable<PixelFilterSettings> settingsList, Texture source, RenderTexture dest0, RenderTexture dest1)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Blitter.filterMaterialPath);

            FirstRender(material, source, dest0);
            var (src, dest) = (dest0, dest1);
            foreach (var settings in settingsList)
            {
                if (settings.toggle)
                {
                    InitMaterial(material, settings);
                    Graphics.Blit(src, dest, material);
                    (src, dest) = (dest, src);
                }
            }
            ResetMaterial(material);
            return src;
        }

        private static void ResetMaterial(Material material)
        {
            var settings = new PixelFilterSettings() { toggle = true, mode = PixelFilterMode.Normal, color = new Color(0F, 0F, 0F, 0F) };
            InitMaterial(material, settings);
        }

        private static void FirstRender(Material material, Texture source, RenderTexture dest)
        {
            ResetMaterial(material);
            Graphics.Blit(source, dest, material);
        }

        private static void InitMaterial(Material material, PixelFilterSettings settings)
        {
            EnableKeyword(material, settings);
            material.SetColor("_Color", settings.color);
        }

        private static void EnableKeyword(Material material, PixelFilterSettings settings)
        {
            foreach (var modeObj in Enum.GetValues(typeof(PixelFilterMode)))
            {
                var mode = (PixelFilterMode)modeObj;
                var keyword = $"_MODE_{Enum.GetName(typeof(PixelFilterMode), mode).ToUpper()}";
                if (mode == settings.mode)
                {
                    material.SetFloat("_Mode", (float)mode);
                    material.EnableKeyword(keyword);
                }
                else
                {
                    material.DisableKeyword(keyword);
                }
            }
        }
    }
}
