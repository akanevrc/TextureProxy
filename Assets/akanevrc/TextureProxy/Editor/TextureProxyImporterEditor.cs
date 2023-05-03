using UnityEditor.Experimental.AssetImporters;
using UnityEditor;
using UnityEngine;

namespace akanevrc.TextureProxy
{
    [CustomEditor(typeof(TextureProxyImporter))]
    public class TextureProxyImporterEditor : ScriptedImporterEditor
    {
        SerializedProperty albedo;

        public override void OnEnable()
        {
            base.OnEnable();
            this.albedo = this.serializedObject.FindProperty(nameof(this.albedo));
        }

        public override void OnInspectorGUI()
        {
            var importer = (TextureProxyImporter)this.target;

            EditorGUI.BeginChangeCheck();

            var oldFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("Texture Wrapper Import Settings");
            EditorStyles.label.fontStyle = oldFontStyle;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.albedo);

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
            base.ApplyRevertGUI();
        }
    }
}
