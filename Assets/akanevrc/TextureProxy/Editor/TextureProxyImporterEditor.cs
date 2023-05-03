using UnityEditor.Experimental.AssetImporters;
using UnityEditor;
using UnityEngine;

namespace akanevrc.TextureProxy
{
    [CustomEditor(typeof(TextureProxyImporter))]
    public class TextureProxyImporterEditor : ScriptedImporterEditor
    {
        TextureProxyImporter importer;
        SerializedProperty settings;

        public override void OnEnable()
        {
            base.OnEnable();
            this.importer = (TextureProxyImporter)this.target;
            this.settings = this.serializedObject.FindProperty(nameof(this.settings));
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            var oldFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("Texture Wrapper Import Settings");
            EditorStyles.label.fontStyle = oldFontStyle;

            EditorGUILayout.Space();

            PixelFilterSettingsField(this.settings);

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
            base.ApplyRevertGUI();
        }

        private void PixelFilterSettingsField(SerializedProperty settings)
        {
            var modeProp = settings.FindPropertyRelative("mode");
            var rProp = settings.FindPropertyRelative("r");
            var gProp = settings.FindPropertyRelative("g");
            var bProp = settings.FindPropertyRelative("b");
            var aProp = settings.FindPropertyRelative("a");

            EditorGUI.BeginChangeCheck();

            var mode = (PixelFilterMode)EditorGUILayout.EnumPopup("Mode", (PixelFilterMode)modeProp.intValue);
            var r = EditorGUILayout.Slider("R", rProp.floatValue, 0F, 1F);
            var g = EditorGUILayout.Slider("G", gProp.floatValue, 0F, 1F);
            var b = EditorGUILayout.Slider("B", bProp.floatValue, 0F, 1F);
            var a = EditorGUILayout.Slider("A", aProp.floatValue, 0F, 1F);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this.importer, "Inspector");
                modeProp.intValue = (int)mode;
                rProp.floatValue = r;
                gProp.floatValue = g;
                bProp.floatValue = b;
                aProp.floatValue = a;
            }
        }
    }
}
