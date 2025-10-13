using UnityEngine;

public class SingleProjectDisplay : ProjectDisplayBase
{
    [Header("Auto-Load Settings")]
    [Tooltip("If true, automatically loads project from Resources folder using resourcePath")]
    public bool autoLoadFromResources = false;

    [Tooltip("Path to ProjectDataAsset in Resources folder (e.g., 'ProjectData/AYARFProject')")]
    public string resourcePath = "ProjectData/AYARFProject";

    protected override void Start()
    {
        // Auto-load from Resources if enabled and no assets are assigned
        if (autoLoadFromResources && (projectAssets == null || projectAssets.Length == 0))
        {
            LoadFromResources();
        }

        base.Start();
    }

    private void LoadFromResources()
    {
        if (string.IsNullOrEmpty(resourcePath))
        {
            Debug.LogError($"SingleProjectDisplay: resourcePath is empty! Set it to load a project (e.g., 'ProjectData/AYARFProject')");
            return;
        }

        ProjectDataAsset asset = Resources.Load<ProjectDataAsset>(resourcePath);
        if (asset != null)
        {
            projectAssets = new ProjectDataAsset[] { asset };
            if (enableLogging)
                Debug.Log($"SingleProjectDisplay: Auto-loaded project from Resources: {resourcePath}");
        }
        else
        {
            Debug.LogError($"SingleProjectDisplay: Failed to load ProjectDataAsset from Resources path: {resourcePath}");
        }
    }

    protected override void OnProjectsLoaded()
    {
        base.OnProjectsLoaded();

        if (projectAssets != null && projectAssets.Length > 1)
        {
            Debug.LogWarning($"SingleProjectDisplay: Multiple projects assigned ({projectAssets.Length}). Only the first one will be displayed. For multiple projects, use TVDisplayController instead.");
        }

        if (enableLogging && currentProject != null)
            Debug.Log($"SingleProjectDisplay: Loaded and displaying project '{currentProject.projectName}'");
    }
}
