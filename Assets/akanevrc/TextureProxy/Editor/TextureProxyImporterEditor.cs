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

        private TextureProxyImporter importer;
        private SerializedProperty pixelFilterSettingsList;
        private SerializedProperty sourceTextureInformation;
        private SerializedProperty textureImporterSettings;
        private SerializedProperty textureImporterPlatformSettings;

        private Texture2D previewTexture;
        private Texture2D sourceTexture;

        public override void OnEnable()
        {
            base.OnEnable();
            this.importer = (TextureProxyImporter)this.target;
            this.pixelFilterSettingsList = this.serializedObject.FindProperty(nameof(this.pixelFilterSettingsList));
            this.sourceTextureInformation = this.serializedObject.FindProperty(nameof(this.sourceTextureInformation));
            this.textureImporterSettings = this.serializedObject.FindProperty(nameof(this.textureImporterSettings));
            this.textureImporterPlatformSettings = this.serializedObject.FindProperty(nameof(this.textureImporterPlatformSettings));

            this.previewTexture =
                new Texture2D
                (
                    (int)TextureProxyImporterEditor.previewTextureSize.x,
                    (int)TextureProxyImporterEditor.previewTextureSize.y,
                    TextureFormat.RGBA32,
                    0,
                    false
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

            var bytes = (byte[])null;
            try
            {
                bytes = File.ReadAllBytes(AssetDatabase.GetAssetPath(this.assetTarget));
            }
            catch (IOException)
            {
                return;
            }

            this.sourceTexture.LoadImage(bytes);
            this.previewTexture.LoadImage(bytes);
            this.previewTexture.SetPixels(PixelFilter.FilterAll(this.importer.pixelFilterSettingsList, this.previewTexture.GetPixels()));
            this.previewTexture.Apply();
        }

        public override void OnDisable()
        {
            if (this.previewTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(this.previewTexture);
                this.previewTexture = null;
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
            var oldFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("Texture Proxy Import Settings");
            EditorStyles.label.fontStyle = oldFontStyle;

            EditorGUILayout.Space();

            var controlRect = EditorGUILayout.GetControlRect(false, TextureProxyImporterEditor.previewTextureSize.y);
            var previewTextureRect = new Rect(controlRect.position, TextureProxyImporterEditor.previewTextureSize);
            var sourceTextureRect =
                new Rect
                (
                    controlRect.position +
                        new Vector2(TextureProxyImporterEditor.previewTextureSize.x + TextureProxyImporterEditor.previewTextureSpace, 0F),
                    TextureProxyImporterEditor.previewTextureSize
                );
            EditorGUI.DrawPreviewTexture(previewTextureRect, this.previewTexture);
            EditorGUI.DrawPreviewTexture(sourceTextureRect, this.sourceTexture);

            EditorGUILayout.Space();

            PixelFilterSettingsList(this.pixelFilterSettingsList);

            EditorGUILayout.Space();

            SourceTextureInformationFields(this.sourceTextureInformation);

            EditorGUILayout.Space();

            TextureImporterSettingsFields(this.textureImporterSettings);

            EditorGUILayout.Space();
            
            TextureImporterPlatformSettingsFields(this.textureImporterPlatformSettings);

            serializedObject.ApplyModifiedProperties();
            base.ApplyRevertGUI();

            if (EditorGUI.EndChangeCheck())
            {
                this.previewTexture.SetPixels(PixelFilter.FilterAll(this.importer.pixelFilterSettingsList, this.sourceTexture.GetPixels()));
                this.previewTexture.Apply();
            }
        }

        private void PixelFilterSettingsList(SerializedProperty settingsList)
        {
            EditorGUI.BeginChangeCheck();

            var movingUpIndex = new List<int>();
            var movingDownIndex = new List<int>();
            var insertingIndex = new List<int>();
            var deletingIndex = new List<int>();
            for (var i = settingsList.arraySize - 1; i >= 0; i--)
            {
                var settings = settingsList.GetArrayElementAtIndex(i);
                var toggle = settings.FindPropertyRelative("toggle");

                toggle.boolValue = EditorGUILayout.BeginToggleGroup($"Layer {i}", toggle.boolValue);
                PixelFilterSettingsFields(settings);
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
            }
            foreach (var index in deletingIndex)
            {
                settingsList.DeleteArrayElementAtIndex(index);
            }

            if (GUILayout.Button("Add"))
            {
                settingsList.InsertArrayElementAtIndex(settingsList.arraySize);
            }
        }

        private void PixelFilterSettingsFields(SerializedProperty settings)
        {
            var mode = settings.FindPropertyRelative("mode");
            var r = settings.FindPropertyRelative("r");
            var g = settings.FindPropertyRelative("g");
            var b = settings.FindPropertyRelative("b");
            var a = settings.FindPropertyRelative("a");

            mode.intValue = (int)(PixelFilterMode)EditorGUILayout.EnumPopup("Mode", (PixelFilterMode)mode.intValue);
            r.floatValue = EditorGUILayout.Slider("R", r.floatValue, 0F, 1F);
            g.floatValue = EditorGUILayout.Slider("G", g.floatValue, 0F, 1F);
            b.floatValue = EditorGUILayout.Slider("B", b.floatValue, 0F, 1F);
            a.floatValue = EditorGUILayout.Slider("A", a.floatValue, 0F, 1F);
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

        private void TextureImporterSettingsFields(SerializedProperty settings)
        {
            var sRGBTexture = settings.FindPropertyRelative("sRGBTexture");
            var alphaSource = settings.FindPropertyRelative("alphaSource");
            var alphaIsTransparency = settings.FindPropertyRelative("alphaIsTransparency");
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

            sRGBTexture.boolValue = EditorGUILayout.Toggle("sRGB Texture", sRGBTexture.boolValue);
            alphaSource.intValue = (int)(TextureImporterAlphaSource)EditorGUILayout.EnumPopup("Alpha Source", (TextureImporterAlphaSource)alphaSource.intValue);
            if ((TextureImporterAlphaSource)alphaSource.intValue != TextureImporterAlphaSource.None)
            {
                EditorGUI.indentLevel++;
                alphaIsTransparency.boolValue = EditorGUILayout.Toggle("Alpha Is Transparency", alphaIsTransparency.boolValue);
                EditorGUI.indentLevel--;
            }
            readable.boolValue = EditorGUILayout.Toggle("Readable", readable.boolValue);
            streamingMipmaps.boolValue = EditorGUILayout.Toggle("Streaming Mipmaps", streamingMipmaps.boolValue);
            if (streamingMipmaps.boolValue)
            {
                EditorGUI.indentLevel++;
                streamingMipmapsPriority.intValue = EditorGUILayout.IntField("Streaming Mipmaps Priority", streamingMipmapsPriority.intValue);
                EditorGUI.indentLevel--;
            }
            mipmapEnabled.boolValue = EditorGUILayout.Toggle("Mipmap Enabled", mipmapEnabled.boolValue);
            if (mipmapEnabled.boolValue)
            {
                EditorGUI.indentLevel++;
                borderMipmap.boolValue = EditorGUILayout.Toggle("Border Mipmap", borderMipmap.boolValue);
                mipmapFilter.intValue = (int)(TextureImporterMipFilter)EditorGUILayout.EnumPopup("Mipmap Filter", (TextureImporterMipFilter)mipmapFilter.intValue);
                mipMapsPreserveCoverage.boolValue = EditorGUILayout.Toggle("Mip Maps Preserve Coverage", mipMapsPreserveCoverage.boolValue);
                if (mipMapsPreserveCoverage.boolValue)
                {
                    EditorGUI.indentLevel++;
                    alphaTestReferenceValue.floatValue = EditorGUILayout.FloatField("Alpha Test Reference Value", alphaTestReferenceValue.floatValue);
                    EditorGUI.indentLevel--;
                }
                fadeOut.boolValue = EditorGUILayout.Toggle("Fade Out", fadeOut.boolValue);
                if (fadeOut.boolValue)
                {
                    EditorGUI.indentLevel++;
                    mipmapFadeDistanceStart.intValue = EditorGUILayout.IntSlider("Mipmap Fade Distance Start", mipmapFadeDistanceStart.intValue, 0, mipmapFadeDistanceEnd.intValue);
                    mipmapFadeDistanceEnd.intValue = EditorGUILayout.IntSlider("Mipmap Fade Distance End", mipmapFadeDistanceEnd.intValue, mipmapFadeDistanceStart.intValue, 10);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            wrapMode.intValue = (int)(TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", (TextureWrapMode)wrapMode.intValue);
            filterMode.intValue = (int)(FilterMode)EditorGUILayout.EnumPopup("Filter Mode", (FilterMode)filterMode.intValue);
            aniso.intValue = EditorGUILayout.IntSlider("Aniso", aniso.intValue, 0, 16);
        }

        private void TextureImporterPlatformSettingsFields(SerializedProperty settings)
        {
            var format = settings.FindPropertyRelative("format");
            var maxTextureSize = settings.FindPropertyRelative("maxTextureSize");
            var resizeAlgorithm = settings.FindPropertyRelative("resizeAlgorithm");
            var textureCompression = settings.FindPropertyRelative("textureCompression");

            format.intValue = (int)(TextureImporterFormat)EditorGUILayout.EnumPopup("Texture Type", (TextureImporterFormat)format.intValue);
            maxTextureSize.intValue = EditorGUILayout.IntField("Texture Shape", maxTextureSize.intValue);
            resizeAlgorithm.intValue = (int)(TextureResizeAlgorithm)EditorGUILayout.EnumPopup("sRGB Texture", (TextureResizeAlgorithm)resizeAlgorithm.intValue);
            textureCompression.intValue = (int)(TextureImporterCompression)EditorGUILayout.EnumPopup("Alpha Source", (TextureImporterCompression)textureCompression.intValue);
        }
    }
}
