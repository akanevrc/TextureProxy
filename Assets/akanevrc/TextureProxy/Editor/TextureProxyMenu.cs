using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace akanevrc.TextureProxy
{
    public static class TextureProxyMenu
    {
        [MenuItem("Assets/Texture Proxy/Duplicate As Texture Proxy")]
        public static void SetTextureProxy()
        {
            var texture = (Texture)Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(texture);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);

            if (!TextureProxyImporter.SupportSettings(importer, out var errors))
            {
                EditorUtility.DisplayDialog("Error: Duplicate As Texture Proxy", string.Join(Environment.NewLine, errors), "OK");
            }

            TextureProxyImporter.activeTexture = texture;
            TextureProxyImporter.activeImporter = importer;

            try
            {
                var newPath =
                    EditorUtility.SaveFilePanelInProject
                    (
                        "Duplicate As Texture Proxy",
                        $"{Path.GetFileName(path)}.texproxy",
                        "texproxy",
                        "Input asset file name",
                        Path.GetDirectoryName(path)
                    );
                if (string.IsNullOrWhiteSpace(newPath)) return;

                AssetDatabase.DeleteAsset(newPath);
                File.Copy(path, newPath);
                AssetDatabase.ImportAsset(newPath);
                AssetDatabase.Refresh();
            }
            finally
            {
                TextureProxyImporter.activeTexture = null;
                TextureProxyImporter.activeImporter = null;
            }
        }

        [MenuItem("Assets/Texture Proxy/Duplicate As Texture Proxy", true)]
        public static bool ValidateSetTextureProxy()
        {
            return
                Selection.activeObject is Texture texture &&
                AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) is TextureImporter;
        }
    }
}
