using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace akanevrc.TextureProxy
{
    [CustomEditor(typeof(TextureProxyImporter))]
    public class TextureProxyImporterEditor : ScriptedImporterEditor
    {
        public static readonly Vector2 previewTextureSize = new Vector2(64F, 64F);
        public static readonly float previewTextureSpace = 8F;

        private SerializedProperty filterSettingsList;
        private SerializedProperty sourceTextureInformation;
        private SerializedProperty textureImporterSettings;
        private SerializedProperty textureImporterPlatformSettings;

        private RenderTexture previewTexture;
        private RenderTexture renderTexture0;
        private RenderTexture renderTexture1;
        private Texture2D sourceTexture;
        private bool importSettingsFoldout;

        public override void OnEnable()
        {
            base.OnEnable();
            this.filterSettingsList = this.serializedObject.FindProperty(nameof(this.filterSettingsList));
            this.sourceTextureInformation = this.serializedObject.FindProperty(nameof(this.sourceTextureInformation));
            this.textureImporterSettings = this.serializedObject.FindProperty(nameof(this.textureImporterSettings));
            this.textureImporterPlatformSettings = this.serializedObject.FindProperty(nameof(this.textureImporterPlatformSettings));

            this.renderTexture0 =
                RenderTexture.GetTemporary
                (
                    (int)TextureProxyImporterEditor.previewTextureSize.x,
                    (int)TextureProxyImporterEditor.previewTextureSize.y,
                    0
                );
            this.renderTexture1 =
                RenderTexture.GetTemporary
                (
                    (int)TextureProxyImporterEditor.previewTextureSize.x,
                    (int)TextureProxyImporterEditor.previewTextureSize.y,
                    0
                );
            this.sourceTexture =
                new Texture2D
                (
                    (int)TextureProxyImporterEditor.previewTextureSize.x,
                    (int)TextureProxyImporterEditor.previewTextureSize.y,
                    TextureFormat.RGBA32,
                    0,
                    false
                );

            this.previewTexture = this.renderTexture0;

            var bytes = (byte[])null;
            try
            {
                bytes = File.ReadAllBytes(AssetDatabase.GetAssetPath(this.assetTarget));
            }
            catch (IOException)
            {
                return;
            }

            var importer = (TextureProxyImporter)this.target;
            this.sourceTexture.LoadImage(bytes);
            this.previewTexture = Blitter.Blit(importer.filterSettingsList, this.sourceTexture, this.renderTexture0, this.renderTexture1);
        }

        public override void OnDisable()
        {
            if (this.renderTexture0 != null)
            {
                RenderTexture.ReleaseTemporary(this.renderTexture0);
                this.renderTexture0 = null;
            }
            if (this.renderTexture1 != null)
            {
                RenderTexture.ReleaseTemporary(this.renderTexture1);
                this.renderTexture1 = null;
            }
            if (this.sourceTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(this.sourceTexture);
                this.sourceTexture = null;
            }
            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            var importer = (TextureProxyImporter)this.target;

            var changed = false;
            EditorGUI.BeginChangeCheck();

            var oldFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("Texture Proxy Import Settings");
            EditorStyles.label.fontStyle = oldFontStyle;

            EditorGUILayout.Space();

            var controlRect = EditorGUILayout.GetControlRect(false, TextureProxyImporterEditor.previewTextureSize.y);
            var previewTextureRect = new Rect(controlRect.position, TextureProxyImporterEditor.previewTextureSize);
            EditorGUI.DrawPreviewTexture(previewTextureRect, this.previewTexture);

            EditorGUILayout.Space();

            FilterSettingsList(this.filterSettingsList);

            EditorGUILayout.Space();

            this.importSettingsFoldout = EditorGUILayout.Foldout(this.importSettingsFoldout, "Texture Import Settings");
            if (this.importSettingsFoldout)
            {
                SourceTextureInformationFields(this.sourceTextureInformation);

                EditorGUILayout.Space();

                TextureImporterSettingsFields(this.textureImporterSettings, IsNPOT(this.sourceTextureInformation));

                EditorGUILayout.Space();
                
                TextureImporterPlatformSettingsFields(this.textureImporterPlatformSettings);
            }

            if (EditorGUI.EndChangeCheck()) changed = true;

            serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                this.previewTexture = Blitter.Blit(importer.filterSettingsList, this.sourceTexture, this.renderTexture0, this.renderTexture1);
            }

            base.ApplyRevertGUI();
        }

        private void FilterSettingsList(SerializedProperty settingsList)
        {
            var movingUpIndex = new List<int>();
            var movingDownIndex = new List<int>();
            var insertingIndex = new List<int>();
            var deletingIndex = new List<int>();
            for (var i = settingsList.arraySize - 1; i >= 0; i--)
            {
                var settings = settingsList.GetArrayElementAtIndex(i);
                var toggle = settings.FindPropertyRelative("toggle");

                toggle.boolValue = EditorGUILayout.BeginToggleGroup($"Layer {i}", toggle.boolValue);
                FilterSettingsFields(settings);
                EditorGUILayout.EndToggleGroup();

                if (i < settingsList.arraySize - 1 && GUILayout.Button("Move Up"))
                {
                    movingUpIndex.Add(i);
                }
                if (i > 0 && GUILayout.Button("Move Down"))
                {
                    movingDownIndex.Add(i);
                }
                if (GUILayout.Button("Insert"))
                {
                    insertingIndex.Add(i);
                }
                if (GUILayout.Button("Delete"))
                {
                    deletingIndex.Add(i);
                }
            }

            foreach (var index in movingUpIndex)
            {
                settingsList.MoveArrayElement(index, index + 1);
            }
            foreach (var index in movingDownIndex.Reverse<int>())
            {
                settingsList.MoveArrayElement(index, index - 1);
            }
            foreach (var index in insertingIndex)
            {
                settingsList.InsertArrayElementAtIndex(index);
                InitFilterSettings(settingsList.GetArrayElementAtIndex(index));
            }
            foreach (var index in deletingIndex)
            {
                settingsList.DeleteArrayElementAtIndex(index);
            }

            if (GUILayout.Button("Add"))
            {
                settingsList.InsertArrayElementAtIndex(settingsList.arraySize);
                InitFilterSettings(settingsList.GetArrayElementAtIndex(settingsList.arraySize - 1));
            }
        }

        private void InitFilterSettings(SerializedProperty settings)
        {
            var toggle = settings.FindPropertyRelative("toggle");
            var mode = settings.FindPropertyRelative("mode");
            var colorTexture = settings.FindPropertyRelative("colorTexture");
            var colorTextureScale = settings.FindPropertyRelative("colorTextureScale");
            var colorTextureOffset = settings.FindPropertyRelative("colorTextureOffset");
            var maskTexture = settings.FindPropertyRelative("maskTexture");
            var maskTextureScale = settings.FindPropertyRelative("maskTextureScale");
            var maskTextureOffset = settings.FindPropertyRelative("maskTextureOffset");
            var color = settings.FindPropertyRelative("color");
            var hue = settings.FindPropertyRelative("hue");
            var saturation = settings.FindPropertyRelative("saturation");
            var luminosity = settings.FindPropertyRelative("luminosity");
            var gamma = settings.FindPropertyRelative("gamma");

            toggle.boolValue = true;
            mode.intValue = (int)FilterMode.ColorCorrection;
            colorTexture.objectReferenceValue = null;
            colorTextureScale.vector2Value = new Vector2(1F, 1F);
            colorTextureOffset.vector2Value = new Vector2(0F, 0F);
            maskTexture.objectReferenceValue = null;
            maskTextureScale.vector2Value = new Vector2(1F, 1F);
            maskTextureOffset.vector2Value = new Vector2(0F, 0F);
            color.colorValue = Color.white;
            hue.floatValue = 0F;
            saturation.floatValue = 0F;
            luminosity.floatValue = 0F;
            gamma.floatValue = 0F;
        }

        private void FilterSettingsFields(SerializedProperty settings)
        {
            var mode = settings.FindPropertyRelative("mode");
            var colorTexture = settings.FindPropertyRelative("colorTexture");
            var colorTextureScale = settings.FindPropertyRelative("colorTextureScale");
            var colorTextureOffset = settings.FindPropertyRelative("colorTextureOffset");
            var maskTexture = settings.FindPropertyRelative("maskTexture");
            var maskTextureScale = settings.FindPropertyRelative("maskTextureScale");
            var maskTextureOffset = settings.FindPropertyRelative("maskTextureOffset");
            var color = settings.FindPropertyRelative("color");
            var hue = settings.FindPropertyRelative("hue");
            var saturation = settings.FindPropertyRelative("saturation");
            var luminosity = settings.FindPropertyRelative("luminosity");
            var gamma = settings.FindPropertyRelative("gamma");

            mode.intValue = (int)(FilterMode)EditorGUILayout.EnumPopup("Mode", (FilterMode)mode.intValue);
            EditorGUILayout.Space();

            if (mode.intValue == (int)FilterMode.ColorCorrection)
            {
                hue.floatValue = EditorGUILayout.Slider("Hue", hue.floatValue, -180F, 180F);
                saturation.floatValue = EditorGUILayout.Slider("Saturation", saturation.floatValue, -1F, 1F);
                luminosity.floatValue = EditorGUILayout.Slider("Luminosity", luminosity.floatValue, -1F, 1F);
                color.colorValue =
                    new Color(
                        color.colorValue.r,
                        color.colorValue.g,
                        color.colorValue.b,
                        EditorGUILayout.Slider("Alpha", color.colorValue.a, 0F, 1F)
                    );
            }
            if (mode.intValue == (int)FilterMode.GammaCorrection)
            {
                luminosity.floatValue = EditorGUILayout.Slider("Luminosity", luminosity.floatValue, -1F, 1F);
                gamma.floatValue = EditorGUILayout.Slider("Gamma Value", gamma.floatValue, -10F, 10F);
                color.colorValue =
                    new Color(
                        color.colorValue.r,
                        color.colorValue.g,
                        color.colorValue.b,
                        EditorGUILayout.Slider("Alpha", color.colorValue.a, 0F, 1F)
                    );
            }
            else
            {
                TextureField("Color Texture", colorTexture, colorTextureScale, colorTextureOffset);
                EditorGUILayout.Space();
                TextureField("Mask", maskTexture, maskTextureScale, maskTextureOffset);
                EditorGUILayout.Space();
                color.colorValue = EditorGUILayout.ColorField("Color", color.colorValue);
            }
        }

        private void TextureField(string label, SerializedProperty texture, SerializedProperty scale, SerializedProperty offset)
        {
            EditorGUILayout.LabelField(label);
            EditorGUI.indentLevel++;
            var rect = EditorGUILayout.GetControlRect(true, 0F);
            rect.height = 42F;
            texture.objectReferenceValue = EditorGUI.ObjectField(rect, "", texture.objectReferenceValue, typeof(Texture2D), false);
            scale.vector2Value = EditorGUILayout.Vector2Field("Tiling", scale.vector2Value);
            offset.vector2Value = EditorGUILayout.Vector2Field("Offset", offset.vector2Value);
            EditorGUI.indentLevel--;
        }

        private void SourceTextureInformationFields(SerializedProperty information)
        {
            var width = information.FindPropertyRelative("width");
            var height = information.FindPropertyRelative("height");
            var containsAlpha = information.FindPropertyRelative("containsAlpha");
            var hdr = information.FindPropertyRelative("hdr");

            width.intValue = EditorGUILayout.IntField("Width", width.intValue);
            height.intValue = EditorGUILayout.IntField("Height", height.intValue);
            containsAlpha.boolValue = EditorGUILayout.Toggle("Contains Alpha", containsAlpha.boolValue);
            hdr.boolValue = EditorGUILayout.Toggle("HDR", hdr.boolValue);
        }

        private bool IsNPOT(SerializedProperty information)
        {
            var width = information.FindPropertyRelative("width");
            var height = information.FindPropertyRelative("height");
            return Mathf.NextPowerOfTwo(width.intValue) != width.intValue || Mathf.NextPowerOfTwo(height.intValue) != height.intValue;
        }

        private void TextureImporterSettingsFields(SerializedProperty settings, bool npot)
        {
            var sRGBTexture = settings.FindPropertyRelative("sRGBTexture");
            var alphaSource = settings.FindPropertyRelative("alphaSource");
            var alphaIsTransparency = settings.FindPropertyRelative("alphaIsTransparency");
            var npotScale = settings.FindPropertyRelative("npotScale");
            var readable = settings.FindPropertyRelative("readable");
            var streamingMipmaps = settings.FindPropertyRelative("streamingMipmaps");
            var streamingMipmapsPriority = settings.FindPropertyRelative("streamingMipmapsPriority");
            var mipmapEnabled = settings.FindPropertyRelative("mipmapEnabled");
            var borderMipmap = settings.FindPropertyRelative("borderMipmap");
            var mipmapFilter = settings.FindPropertyRelative("mipmapFilter");
            var mipMapsPreserveCoverage = settings.FindPropertyRelative("mipMapsPreserveCoverage");
            var alphaTestReferenceValue = settings.FindPropertyRelative("alphaTestReferenceValue");
            var fadeOut = settings.FindPropertyRelative("fadeOut");
            var mipmapFadeDistanceStart = settings.FindPropertyRelative("mipmapFadeDistanceStart");
            var mipmapFadeDistanceEnd = settings.FindPropertyRelative("mipmapFadeDistanceEnd");
            var wrapMode = settings.FindPropertyRelative("wrapMode");
            var filterMode = settings.FindPropertyRelative("filterMode");
            var aniso = settings.FindPropertyRelative("aniso");

            sRGBTexture.boolValue = EditorGUILayout.Toggle("sRGB (Color Texture)", sRGBTexture.boolValue);
            alphaSource.intValue = (int)(TextureImporterAlphaSource)EditorGUILayout.EnumPopup("Alpha Source", (TextureImporterAlphaSource)alphaSource.intValue);
            if ((TextureImporterAlphaSource)alphaSource.intValue != TextureImporterAlphaSource.None)
            {
                EditorGUI.indentLevel++;
                alphaIsTransparency.boolValue = EditorGUILayout.Toggle("Alpha Is Transparency", alphaIsTransparency.boolValue);
                EditorGUI.indentLevel--;
            }
            if (npot)
            {
                npotScale.intValue = (int)(TextureImporterNPOTScale)EditorGUILayout.EnumPopup("Non-Power of 2", (TextureImporterNPOTScale)npotScale.intValue);
            }
            readable.boolValue = EditorGUILayout.Toggle("Read/Write Enabled", readable.boolValue);
            streamingMipmaps.boolValue = EditorGUILayout.Toggle("Streaming Mipmaps", streamingMipmaps.boolValue);
            if (streamingMipmaps.boolValue)
            {
                EditorGUI.indentLevel++;
                streamingMipmapsPriority.intValue = EditorGUILayout.IntField("Mip Map Priority", streamingMipmapsPriority.intValue);
                EditorGUI.indentLevel--;
            }
            else
            {
                mipmapEnabled.boolValue = false;
            }
            mipmapEnabled.boolValue = EditorGUILayout.Toggle("Generate Mip Maps", mipmapEnabled.boolValue);
            if (mipmapEnabled.boolValue)
            {
                streamingMipmaps.boolValue = true;

                EditorGUI.indentLevel++;
                borderMipmap.boolValue = EditorGUILayout.Toggle("Border Mip Maps", borderMipmap.boolValue);
                mipmapFilter.intValue = (int)(TextureImporterMipFilter)EditorGUILayout.EnumPopup("Mip Map Filtering", (TextureImporterMipFilter)mipmapFilter.intValue);
                mipMapsPreserveCoverage.boolValue = EditorGUILayout.Toggle("Mip Maps Preserve Coverage", mipMapsPreserveCoverage.boolValue);
                if (mipMapsPreserveCoverage.boolValue)
                {
                    EditorGUI.indentLevel++;
                    alphaTestReferenceValue.floatValue = EditorGUILayout.FloatField("Alpha Cutoff Value", alphaTestReferenceValue.floatValue);
                    EditorGUI.indentLevel--;
                }
                fadeOut.boolValue = EditorGUILayout.Toggle("Fadeout Mip Maps", fadeOut.boolValue);
                if (fadeOut.boolValue)
                {
                    EditorGUI.indentLevel++;
                    mipmapFadeDistanceStart.intValue = EditorGUILayout.IntSlider("Fade Range Start", mipmapFadeDistanceStart.intValue, 0, mipmapFadeDistanceEnd.intValue);
                    mipmapFadeDistanceEnd.intValue = EditorGUILayout.IntSlider("Fade Range End", mipmapFadeDistanceEnd.intValue, mipmapFadeDistanceStart.intValue, 10);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            wrapMode.intValue = (int)(TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", (TextureWrapMode)wrapMode.intValue);
            filterMode.intValue = (int)(UnityEngine.FilterMode)EditorGUILayout.EnumPopup("Filter Mode", (UnityEngine.FilterMode)filterMode.intValue);
            aniso.intValue = EditorGUILayout.IntSlider("Aniso Level", aniso.intValue, 0, 16);
        }

        private void TextureImporterPlatformSettingsFields(SerializedProperty settings)
        {
            var maxTextureSize = settings.FindPropertyRelative("maxTextureSize");
            var resizeAlgorithm = settings.FindPropertyRelative("resizeAlgorithm");
            var format = settings.FindPropertyRelative("format");
            var textureCompression = settings.FindPropertyRelative("textureCompression");
            var crunchedCompression = settings.FindPropertyRelative("crunchedCompression");

            maxTextureSize.intValue = EditorGUILayout.IntField("Max Size", maxTextureSize.intValue);
            resizeAlgorithm.intValue = (int)(TextureResizeAlgorithm)EditorGUILayout.EnumPopup("Resize Algorithm", (TextureResizeAlgorithm)resizeAlgorithm.intValue);
            format.intValue = (int)(TextureImporterFormat)EditorGUILayout.EnumPopup("Format", (TextureImporterFormat)format.intValue);
            textureCompression.intValue = (int)(TextureImporterCompression)EditorGUILayout.EnumPopup("Texture Compression", (TextureImporterCompression)textureCompression.intValue);
            if ((TextureImporterCompression)textureCompression.intValue != TextureImporterCompression.Uncompressed)
            {
                EditorGUI.indentLevel++;
                crunchedCompression.boolValue = EditorGUILayout.Toggle("Use Crunch Compression", crunchedCompression.boolValue);
                EditorGUI.indentLevel--;
            }
        }
    }
}
