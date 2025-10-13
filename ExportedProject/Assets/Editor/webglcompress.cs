using UnityEngine;
using UnityEditor;

public class WebGLTextureCompressor : EditorWindow
{
    private string[] guids;
    private int index = 0;
    private int batchSize = 50; // how many textures per step

    [MenuItem("Tools/Compress All Textures for WebGL")]
    public static void ShowWindow()
    {
        GetWindow<WebGLTextureCompressor>("WebGL Texture Compressor").Init();
    }

    void Init()
    {
        guids = AssetDatabase.FindAssets("t:Texture2D");
        index = 0;
        EditorApplication.update += ProcessBatch;
    }

    void ProcessBatch()
    {
        if (index >= guids.Length)
        {
            EditorApplication.update -= ProcessBatch;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ Finished compressing all textures for WebGL");
            return;
        }

        int end = Mathf.Min(index + batchSize, guids.Length);

        for (int i = index; i < end; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            // WebGL override
            var settings = importer.GetPlatformTextureSettings("WebGL");
            // settings.overridden = false;
            // settings.maxTextureSize = 1024; // reduce size
            // settings.format = TextureImporterFormat.ASTC_6x6; // good quality/size
            // settings.compressionQuality = 50;

            settings.overridden = true;
            settings.maxTextureSize = 4096; // reduce size
            settings.format = TextureImporterFormat.Automatic; // good quality/size
            settings.compressionQuality = 100;
            importer.SetPlatformTextureSettings(settings);
            EditorUtility.SetDirty(importer);
        }

        Debug.Log($"Processed {end}/{guids.Length} textures...");
        index = end;
    }
}
