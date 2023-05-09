using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace akanevrc.TextureProxy
{
    public static class TextureProxyMenu
    {
        [MenuItem("Assets/Texture Proxy/Duplicate As Texture Proxy")]
        public static void DuplicateTextureProxy()
        {
            var texture = (Texture)Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(texture);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);

            if (!TextureProxyImporter.SupportSettings(importer, out var errors))
            {
                EditorUtility.DisplayDialog("Error: Duplicate As Texture Proxy", string.Join(Environment.NewLine, errors), "OK");
                return;
            }

            var newPath =
                EditorUtility.SaveFilePanelInProject
                (
                    "Duplicate As Texture Proxy",
                    TextureProxyFileName(Path.GetFileName(path)),
                    "texproxy",
                    "Input asset file name",
                    Path.GetDirectoryName(path)
                );
            if (string.IsNullOrWhiteSpace(newPath)) return;

            TextureProxyImporter.activeTexture = texture;
            TextureProxyImporter.activeImporter = importer;

            try
            {
                AssetDatabase.DeleteAsset(newPath);
                File.Copy(path, newPath, true);
                AssetDatabase.ImportAsset(newPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                TextureProxyImporter.activeTexture = null;
                TextureProxyImporter.activeImporter = null;
            }
        }

        [MenuItem("Assets/Texture Proxy/Duplicate As Texture Proxy", true)]
        public static bool ValidateDuplicateTextureProxy()
        {
            return
                Selection.activeObject is Texture texture &&
                AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) is TextureImporter;
        }

        [MenuItem("Assets/Texture Proxy/Duplicate Material")]
        public static void DuplicateMaterial()
        {
            var material = (Material)Selection.activeObject;
            var names =
                material.GetTexturePropertyNames()
                .ToArray();
            var textures =
                names
                .Select(name => material.GetTexture(name))
                .ToArray();

            if (textures.Length == 0)
            {
                EditorUtility.DisplayDialog("Error: Duplicate Material", "No textures found in this material.", "OK");
                return;
            }

            var textureNames =
                names
                .Zip(textures, (name, texture) => (name, texture))
                .Where(z => z.texture != null);
            var textureGroups =
                textures
                .Where(t => t != null)
                .GroupJoin(textureNames, t => t.GetInstanceID(), z => z.texture.GetInstanceID(), (t, zs) => (t, zs))
                .ToArray();
            var paths =
                textureGroups
                .Select(z => (z.t, path: AssetDatabase.GetAssetPath(z.t), z.zs))
                .ToArray();
            var importers =
                paths
                .Select(z => (z.t, z.path, importer: AssetImporter.GetAtPath(z.path), z.zs))
                .ToArray();
            var supporteds =
                importers
                .Where(z => z.importer is TextureImporter importer && TextureProxyImporter.SupportSettings(importer, out var _))
                .ToArray();

            if (supporteds.Length == 0)
            {
                EditorUtility.DisplayDialog("Error: Duplicate Material", "No supported textures found in this material.", "OK");
                return;
            }

            var materialPath = AssetDatabase.GetAssetPath(material);
            var newMaterialPath =
                EditorUtility.SaveFilePanelInProject
                (
                    "Duplicate Material",
                    $"Duplicated_{Path.GetFileName(materialPath)}",
                    "mat",
                    "Input asset file name",
                    Path.GetDirectoryName(materialPath)
                );
            if (string.IsNullOrWhiteSpace(newMaterialPath)) return;
            var dirPath =
                EditorUtility.OpenFolderPanel
                (
                    "Select a Folder to Duplicate Textures",
                    Path.GetDirectoryName(newMaterialPath),
                    ""
                );
            if (string.IsNullOrWhiteSpace(dirPath)) return;
            if (dirPath.StartsWith(Application.dataPath))
            {
                dirPath = "Assets" + dirPath.Substring(Application.dataPath.Length);
            }

            AssetDatabase.DeleteAsset(newMaterialPath);
            if (!AssetDatabase.CopyAsset(materialPath, newMaterialPath))
            {
                throw new IOException($"Fail to copy {materialPath} to {newMaterialPath}");
            }
            AssetDatabase.Refresh();
            var m = AssetDatabase.LoadAssetAtPath<Material>(newMaterialPath);

            foreach (var (t, path, importer, zs) in supporteds)
            {
                var newPath = Path.Combine(dirPath, TextureProxyFileName(Path.GetFileName(path)));

                TextureProxyImporter.activeTexture = t;
                TextureProxyImporter.activeImporter = (TextureImporter)importer;

                try
                {
                    AssetDatabase.DeleteAsset(newPath);
                    File.Copy(path, newPath, true);
                    AssetDatabase.ImportAsset(newPath);
                    AssetDatabase.Refresh();
                }
                finally
                {
                    TextureProxyImporter.activeTexture = null;
                    TextureProxyImporter.activeImporter = null;
                }

                var textureProxy = AssetDatabase.LoadAssetAtPath<Texture>(newPath);

                foreach (var (name, _) in zs)
                {
                    m.SetTexture(name, textureProxy);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Texture Proxy/Duplicate Material", true)]
        public static bool ValidateDuplicateMaterial()
        {
            return Selection.activeObject is Material;
        }

        private static string TextureProxyFileName(string path)
        {
            return $"{path}.texproxy";
        }
    }
}
