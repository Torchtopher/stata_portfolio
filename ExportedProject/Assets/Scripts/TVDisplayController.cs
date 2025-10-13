using UnityEngine;

public class TVDisplayController : ProjectDisplayBase
{
    [Header("Project Rotation Settings")]
    public bool rotateProjects = true;

    [Header("Auto-Load Settings")]
    [Tooltip("If true, automatically loads ALL projects from Resources/ProjectData folder")]
    public bool autoLoadAllProjects = true;

    private int currentProjectIndex = 0;
    private float lastRotationTime;

    protected override void Start()
    {
        // Auto-load all projects from Resources if enabled and no assets are assigned
        if (autoLoadAllProjects && (projectAssets == null || projectAssets.Length == 0))
        {
            LoadAllProjectsFromResources();
        }

        base.Start();
    }

    private void LoadAllProjectsFromResources()
    {
        ProjectDataAsset[] allAssets = Resources.LoadAll<ProjectDataAsset>("ProjectData");

        if (allAssets != null && allAssets.Length > 0)
        {
            projectAssets = allAssets;
            if (enableLogging)
                Debug.Log($"TVDisplayController: Auto-loaded {allAssets.Length} projects from Resources/ProjectData");
        }
        else
        {
            Debug.LogWarning($"TVDisplayController: No ProjectDataAsset files found in Resources/ProjectData folder");
        }
    }

    protected override void OnProjectsLoaded()
    {
        base.OnProjectsLoaded();
        StartDisplayRotation();
    }

    protected override void Update()
    {
        base.Update();

        if (currentThumbnail == null || tvMaterial == null)
            return;

        // Skip rotation if we only have one project or rotation is disabled
        if (!rotateProjects || allProjects == null || allProjects.Length <= 1)
            return;

        float rotationInterval = currentProject?.rotationInterval ?? defaultRotationInterval;
        float timeUntilNextRotation = rotationInterval - (Time.time - lastRotationTime);

        // Log when we're planning to rotate (only occasionally to avoid spam)
        if (enableLogging && Mathf.FloorToInt(timeUntilNextRotation) != Mathf.FloorToInt(timeUntilNextRotation + Time.deltaTime) && timeUntilNextRotation > 0)
        {
            Debug.Log($"TVDisplayController: Next rotation in {Mathf.Ceil(timeUntilNextRotation)}s for project '{currentProject?.projectName}'");
        }

        if (Time.time - lastRotationTime >= rotationInterval)
        {
            RotateToNextTexture();
        }
    }

    private void StartDisplayRotation()
    {
        if (currentThumbnail != null)
        {
            lastRotationTime = Time.time;

            float nextRotationTime = (currentProject?.rotationInterval ?? defaultRotationInterval);
            if (enableLogging)
                Debug.Log($"TVDisplayController: Started rotation for '{currentProject?.projectName}'. Next rotation in {nextRotationTime}s");
        }
    }

    private void RotateToNextTexture()
    {
        if (rotateProjects && allProjects != null && allProjects.Length > 1)
        {
            // Move to next project
            int oldIndex = currentProjectIndex;
            currentProjectIndex = (currentProjectIndex + 1) % allProjects.Length;
            SetCurrentProject(currentProjectIndex);
            if (enableLogging)
                Debug.Log($"TVDisplayController: Rotated from project {oldIndex} ({allProjects[oldIndex].projectName}) to {currentProjectIndex} ({allProjects[currentProjectIndex].projectName})");
        }
        else
        {
            // No rotation when rotateProjects is false or only one project
            if (enableLogging)
                Debug.Log($"TVDisplayController: Rotation skipped - rotateProjects={rotateProjects}, project count={allProjects?.Length ?? 0}");
        }
        lastRotationTime = Time.time;
    }
}
