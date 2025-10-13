using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class SceneTextureOrganizer : EditorWindow
{
    private static string moveFolder = "Assets/UnusedForWebGL";

    [MenuItem("Tools/Move Unused Textures for WebGL")]
    public static void MoveUnusedTextures()
    {
        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder(moveFolder))
        {
            AssetDatabase.CreateFolder("Assets", "UnusedForWebGL");
        }

        HashSet<Texture2D> usedTextures = new HashSet<Texture2D>();

        foreach (var renderer in GameObject.FindObjectsOfType<Renderer>())
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat == null) continue;
                if (mat.mainTexture is Texture2D tex)
                    usedTextures.Add(tex);
            }
        }

        // 2. Find all Texture2D assets in the project
        string[] allGuids = AssetDatabase.FindAssets("t:Texture2D");
        int movedCount = 0;

        foreach (string guid in allGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (!usedTextures.Contains(tex))
            {
                string fileName = Path.GetFileName(path);
                string destPath = AssetDatabase.GenerateUniqueAssetPath(moveFolder + "/" + fileName);

                AssetDatabase.MoveAsset(path, destPath);
                movedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ Moved {movedCount} unused textures to {moveFolder}");
    }
}
