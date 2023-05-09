using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace akanevrc.TextureProxy
{
    public static class Blitter
    {
        public static readonly string filterMaterialPath = "Assets/akanevrc/TextureProxy/Editor/Shaders/Filter.mat";

        public static RenderTexture Blit(IEnumerable<FilterSettings> settingsList, Texture source, RenderTexture dest0, RenderTexture dest1)
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
            var settings = new FilterSettings()
            {
                toggle = true,
                mode = FilterMode.Normal,
                colorTexture = null,
                colorTextureScale = new Vector2(1F, 1F),
                colorTextureOffset = new Vector2(0F, 0F),
                maskTexture = null,
                maskTextureScale = new Vector2(1F, 1F),
                maskTextureOffset = new Vector2(0F, 0F),
                color = new Color(0F, 0F, 0F, 0F),
                hue = 0F,
                saturation = 0F,
                luminosity = 0F,
                gamma = 0F
            };
            InitMaterial(material, settings);
        }

        private static void FirstRender(Material material, Texture source, RenderTexture dest)
        {
            ResetMaterial(material);
            Graphics.Blit(source, dest, material);
        }

        private static void InitMaterial(Material material, FilterSettings settings)
        {
            EnableKeyword(material, settings);
            material.SetTexture("_SubTex", settings.colorTexture);
            material.SetTextureScale("_SubTex", settings.colorTextureScale);
            material.SetTextureOffset("_SubTex", settings.colorTextureOffset);
            material.SetTexture("_Mask", settings.maskTexture);
            material.SetTextureScale("_Mask", settings.maskTextureScale);
            material.SetTextureOffset("_Mask", settings.maskTextureOffset);
            material.SetColor("_Color", settings.color);
            material.SetFloat("_Hue", settings.hue);
            material.SetFloat("_Saturation", settings.saturation);
            material.SetFloat("_Luminosity", settings.luminosity);
            material.SetFloat("_Gamma", settings.gamma);
        }

        private static void EnableKeyword(Material material, FilterSettings settings)
        {
            foreach (var modeObj in Enum.GetValues(typeof(FilterMode)))
            {
                var mode = (FilterMode)modeObj;
                var keyword = $"_MODE_{Enum.GetName(typeof(FilterMode), mode).ToUpper()}";
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
