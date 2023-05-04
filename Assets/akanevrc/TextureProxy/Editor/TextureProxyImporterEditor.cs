using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace akanevrc.TextureProxy
{
    [CustomEditor(typeof(TextureProxyImporter))]
    public class TextureProxyImporterEditor : ScriptedImporterEditor
    {
        TextureProxyImporter importer;
        SerializedProperty settingsList;

        public override void OnEnable()
        {
            base.OnEnable();
            this.importer = (TextureProxyImporter)this.target;
            this.settingsList = this.serializedObject.FindProperty(nameof(this.settingsList));
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            var oldFontStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("Texture Wrapper Import Settings");
            EditorStyles.label.fontStyle = oldFontStyle;

            EditorGUILayout.Space();

            PixelFilterSettingsList(this.settingsList);

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
            base.ApplyRevertGUI();
        }

        private void PixelFilterSettingsList(SerializedProperty settingsList)
        {
            var movingUpIndex = new List<int>();
            var movingDownIndex = new List<int>();
            var insertingIndex = new List<int>();
            var deletingIndex = new List<int>();
            for (var i = settingsList.arraySize - 1; i >= 0; i--)
            {
                var settings = settingsList.GetArrayElementAtIndex(i);
                var toggle = settings.FindPropertyRelative("toggle");

                EditorGUI.BeginChangeCheck();
                var t = EditorGUILayout.BeginToggleGroup($"Layer {i}", toggle.boolValue);
                PixelFilterSettingsField(settings);
                EditorGUILayout.EndToggleGroup();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this.importer, "Inspector");
                    toggle.boolValue = t;
                }

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
