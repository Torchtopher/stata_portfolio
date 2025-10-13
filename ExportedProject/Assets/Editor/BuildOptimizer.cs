using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class BuildOptimizer
{
    private const int LargeTextureThresholdPx = 2048;

    [MenuItem("Tools/Build Optimizer/Scan Project")]
    public static void ScanProject()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Build Optimizer Report - " + DateTime.Now);
        sb.AppendLine();

        // 1) Find missing scripts in prefabs
        sb.AppendLine("1) Missing scripts in Prefabs");
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int totalPrefabs = prefabGuids.Length;
        int prefabsWithMissing = 0;
        foreach (var g in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
            bool hasMissing = false;
            foreach (var comp in go.GetComponentsInChildren<Component>(true))
            {
                if (comp == null)
                {
                    hasMissing = true;
                    break;
                }
            }
            if (hasMissing)
            {
                prefabsWithMissing++;
                sb.AppendLine($" - {path}");
            }
        }
        sb.AppendLine($"Found {prefabsWithMissing} prefabs with missing scripts out of {totalPrefabs} prefabs.");
        sb.AppendLine();

        // 2) Scan textures: look for ASTC format on Standalone overrides and big textures
        sb.AppendLine("2) Texture import issues (Standalone ASTC overrides / large textures)");
        var texGuids = AssetDatabase.FindAssets("t:Texture");
        int astcCount = 0;
        int largeCount = 0;
        foreach (var g in texGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            try
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                // Check platform override for Standalone
                try
                {
                    var platformSettings = importer.GetPlatformTextureSettings("Standalone");
                    if (platformSettings != null && platformSettings.overridden)
                    {
                        var fmt = platformSettings.format.ToString();
                        if (fmt.IndexOf("ASTC", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            astcCount++;
                            sb.AppendLine($" - ASTC override on Standalone: {path} -> {fmt}");
                        }
                    }
                }
                catch { /* API differences across Unity versions; ignore safely */ }

                // Check texture dims
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null)
                {
                    if (tex.width >= LargeTextureThresholdPx || tex.height >= LargeTextureThresholdPx)
                    {
                        largeCount++;
                        sb.AppendLine($" - Large texture ({tex.width}x{tex.height}): {path}");
                    }
                }
            }
            catch (Exception ex)
            {
                // non-fatal: continue scanning
                Debug.LogWarning($"BuildOptimizer: failed to analyze texture {path}: {ex.Message}");
            }
        }
        sb.AppendLine($"Found {astcCount} textures with ASTC Standalone overrides, {largeCount} textures >= {LargeTextureThresholdPx}px.");
        sb.AppendLine();

        // 3) Shaders in Resources/shaders (look for common problematic assets)
        sb.AppendLine("3) Shader resources in Resources/shaders folder (may cause Unknown type 'CGProgram' warnings)");
        var shaderPaths = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/Resources/shaders" })
            .Select(AssetDatabase.GUIDToAssetPath).ToArray();
        if (shaderPaths.Length == 0)
        {
            sb.AppendLine(" - No shader resource text assets found under Assets/Resources/shaders/");
        }
        else
        {
            foreach (var p in shaderPaths)
                sb.AppendLine($" - {p}");
            sb.AppendLine($"Found {shaderPaths.Length} shader resource files under Assets/Resources/shaders/ (check shader compatibility / pipeline)");
        }
        sb.AppendLine();

        // 4) General suggestions
        sb.AppendLine("4) Quick prioritized suggestions:");
        sb.AppendLine(" - Remove or reassign missing scripts on GameObjects/prefabs (they slow loads and can bloat build). Use the report above to find prefabs.");
        sb.AppendLine(" - For Standalone/Desktop builds: remove GLES3 from Player Settings or restrict Graphics APIs to OpenGLCore/Vulkan to avoid generating gles3 shader variants.");
        sb.AppendLine(" - Recompress textures for the target platform (Standalone -> DXT/BCn). Avoid ASTC for desktop builds. Turn off Read/Write and Unused MipMaps.");
        sb.AppendLine(" - Enable managed code stripping and engine code stripping in Player settings for release builds.");
        sb.AppendLine(" - Use a ShaderVariantCollection to prewarm shaders and strip unused variants. Consider limiting keywords and disabling unnecessary post-processing effects.");
        sb.AppendLine(" - Clean up large unused assets in Resources folders. Resources forces inclusion into builds.");
        sb.AppendLine();

        // Write report
        var reportPath = Path.Combine(Application.dataPath, "../BuildOptimizer_Report.txt");
        try
        {
            File.WriteAllText(reportPath, sb.ToString());
            Debug.Log($"Build Optimizer: Scan finished. Report written to: {reportPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Build Optimizer: Failed to write report: {ex.Message}");
        }

        Debug.Log(sb.ToString());
    }
}
