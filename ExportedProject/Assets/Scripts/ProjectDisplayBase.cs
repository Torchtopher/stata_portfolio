using UnityEngine;
using System.Collections.Generic;

public abstract class ProjectDisplayBase : MonoBehaviour
{
    [Header("Display Settings")]
    public float defaultRotationInterval = 3.0f;
    public bool enableLogging = false;

    [Header("Bundled Projects")]
    [Tooltip("Assign ProjectDataAsset ScriptableObjects here for instant loading")]
    public ProjectDataAsset[] projectAssets;

    [Header("Interaction")]
    public KeyCode interactionKey = KeyCode.E;
    public KeyCode closeUIKey = KeyCode.Escape;
    public float interactionDistance = 3.0f;
    public ProximityPopup proximityPopup;
    public ProjectDetailUI projectDetailUI;
    public InteractionMarker interactionMarker;

    // Protected fields accessible to subclasses
    protected ProjectData[] allProjects;
    protected Texture2D[] allThumbnails;
    protected Texture2D[][] allDetailTextures;
    protected ProjectData currentProject;
    protected Texture2D currentThumbnail;
    protected Texture2D[] currentDetailTextures;
    protected Renderer tvRenderer;
    protected Material tvMaterial;

    protected Texture2D[] cachedLetterboxedThumbnails;

    protected Camera playerCamera;
    protected bool playerInRange = false;

    protected virtual void OnProjectsLoaded() { }

    protected virtual void Start()
    {
        tvRenderer = GetComponent<Renderer>();
        if (tvRenderer == null)
        {
            Debug.LogError($"{GetType().Name}: No Renderer component found!");
            return;
        }

        tvMaterial = tvRenderer.material;
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();

        if (proximityPopup != null)
        {
            proximityPopup.SetVisible(false);
            proximityPopup.SetInteractionKey(interactionKey);
        }

        if (projectDetailUI != null)
        {
            projectDetailUI.SetCloseKey(closeUIKey);
        }

        LoadProjects();
    }

    protected virtual void Update()
    {
        CheckPlayerInteraction();
    }

    private void LoadProjects()
    {
        if (projectAssets == null || projectAssets.Length == 0)
        {
            Debug.LogWarning($"{GetType().Name}: No project assets assigned! Assign ProjectDataAsset ScriptableObjects in the Inspector.");
            CreateDefaultProject();
            return;
        }

        List<ProjectData> validProjects = new List<ProjectData>();
        List<Texture2D> validThumbnails = new List<Texture2D>();
        List<Texture2D[]> validDetailTextures = new List<Texture2D[]>();

        foreach (ProjectDataAsset asset in projectAssets)
        {
            Debug.Log($"{GetType().Name}: Processing asset '{(asset != null ? asset.name : "NULL")}'");

            if (asset == null)
            {
                Debug.LogWarning($"{GetType().Name}: Null project asset found in projectAssets array");
                continue;
            }

            if (asset.thumbnailTexture == null)
            {
                Debug.LogWarning($"{GetType().Name}: Project '{asset.projectName}' has no thumbnail texture, skipping");
                continue;
            }

            validProjects.Add(asset.ToProjectData());
            validThumbnails.Add(asset.thumbnailTexture);
            validDetailTextures.Add(asset.detailTextures ?? new Texture2D[0]);

            Debug.Log($"{GetType().Name}: ✓ Successfully loaded asset '{asset.name}' (Project: '{asset.projectName}') with {asset.detailTextures?.Length ?? 0} detail images");
        }

        if (validProjects.Count > 0)
        {
            allProjects = validProjects.ToArray();
            allThumbnails = validThumbnails.ToArray();
            allDetailTextures = validDetailTextures.ToArray();

            cachedLetterboxedThumbnails = new Texture2D[allProjects.Length];

            SetCurrentProject(0);

            if (enableLogging)
                Debug.Log($"{GetType().Name}: Successfully loaded {allProjects.Length} projects instantly");

            OnProjectsLoaded();
        }
        else
        {
            Debug.LogWarning($"{GetType().Name}: No valid projects found, creating default");
            CreateDefaultProject();
        }
    }

    protected void CreateDefaultProject()
    {
        currentProject = new ProjectData
        {
            projectName = "Sample Project",
            detailedDescription = "This is a default project created when no project data was found.",
            thumbnailImage = "default.jpg",
            detailImages = new string[] { "default.jpg" },
            rotationInterval = defaultRotationInterval,
            year = "2024"
        };

        Texture2D defaultThumbnail = new Texture2D(512, 512);
        Color[] pixels = new Color[512 * 512];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(0.2f, 0.2f, 0.5f);
        }
        defaultThumbnail.SetPixels(pixels);
        defaultThumbnail.Apply();

        allProjects = new ProjectData[] { currentProject };
        allThumbnails = new Texture2D[] { defaultThumbnail };
        allDetailTextures = new Texture2D[][] { new Texture2D[] { defaultThumbnail } };
        SetCurrentProject(0);
        SetCurrentTexture();
    }


    protected void SetCurrentProject(int projectIndex)
    {
        if (allProjects != null && projectIndex >= 0 && projectIndex < allProjects.Length)
        {
            currentProject = allProjects[projectIndex];
            currentThumbnail = allThumbnails[projectIndex];
            currentDetailTextures = allDetailTextures[projectIndex];

            SetCurrentTexture();

            if (enableLogging)
                Debug.Log($"{GetType().Name}: Set current project to '{currentProject.projectName}' (index {projectIndex})");
        }
    }

    protected void CheckPlayerInteraction()
    {
        if (playerCamera == null) return;

        float distance = Vector3.Distance(playerCamera.transform.position, transform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionDistance;

        if (playerInRange != wasInRange)
        {
            if (proximityPopup != null)
                proximityPopup.SetVisible(playerInRange);
        }

        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            OnInteraction();
        }
    }

    protected virtual void OnInteraction()
    {
        if (interactionMarker != null)
        {
            interactionMarker.Hide();
        }

        string posterId = gameObject.GetInstanceID().ToString();
        bool isNewDiscovery = PosterDiscoveryTracker.Instance.RegisterDiscovery(posterId);

        if (enableLogging && isNewDiscovery)
        {
            Debug.Log($"{GetType().Name}: First-time discovery of poster '{currentProject?.projectName}'");
        }

        if (projectDetailUI != null && currentProject != null)
        {
            projectDetailUI.Show(currentProject, currentDetailTextures);
            if (enableLogging)
                Debug.Log($"{GetType().Name}: Opened detail UI for project '{currentProject.projectName}' with {currentDetailTextures?.Length ?? 0} images");
        }
        else
        {
            if (enableLogging)
            {
                Debug.Log($"{GetType().Name} Interaction: {currentProject?.projectName ?? "No Project"}");
            }
        }
    }

    protected void SetCurrentTexture()
    {
        if (currentThumbnail != null && tvMaterial != null)
        {
            Vector3 quadScale = tvRenderer.transform.localScale;
            float targetAspect = quadScale.x / quadScale.y;

            float sourceAspect = (float)currentThumbnail.width / currentThumbnail.height;

            int projectIndex = System.Array.IndexOf(allThumbnails, currentThumbnail);

            if (Mathf.Abs(targetAspect - sourceAspect) < 0.01f)
            {
                tvMaterial.mainTexture = currentThumbnail;
            }
            else
            {
                if (projectIndex >= 0 && projectIndex < cachedLetterboxedThumbnails.Length && cachedLetterboxedThumbnails[projectIndex] != null)
                {
                    tvMaterial.mainTexture = cachedLetterboxedThumbnails[projectIndex];
                }
                else
                {
                    bool needsExifRotation = currentProject != null &&
                                             currentProject.thumbnailImage != null &&
                                             currentProject.thumbnailImage == "yarf_side_view.jpg";

                    Texture2D letterboxed = CreateLetterboxedTexture(currentThumbnail, targetAspect, needsExifRotation);

                    if (projectIndex >= 0 && projectIndex < cachedLetterboxedThumbnails.Length)
                    {
                        cachedLetterboxedThumbnails[projectIndex] = letterboxed;
                        allThumbnails[projectIndex] = letterboxed;
                        currentThumbnail = letterboxed;
                    }

                    tvMaterial.mainTexture = letterboxed;
                }
            }

            tvMaterial.mainTextureScale = Vector2.one;
            tvMaterial.mainTextureOffset = Vector2.zero;

            if (enableLogging)
                Debug.Log($"{GetType().Name}: Set texture to thumbnail for project '{currentProject?.projectName}'");
        }
    }

    private Texture2D CreateLetterboxedTexture(Texture2D source, float targetAspect, bool needsExifRotation = false)
    {
        float sourceAspect = (float)source.width / source.height;
        Texture2D workingTexture = source;

        if (needsExifRotation)
        {
            workingTexture = RotateTexture90CCW(source);
            sourceAspect = (float)workingTexture.width / workingTexture.height;
        }

        int newWidth, newHeight;
        int imageWidth, imageHeight;
        int offsetX = 0, offsetY = 0;

        if (sourceAspect > targetAspect)
        {
            newWidth = workingTexture.width;
            newHeight = Mathf.RoundToInt(workingTexture.width / targetAspect);
            imageWidth = workingTexture.width;
            imageHeight = workingTexture.height;
            offsetX = 0;
            offsetY = (newHeight - imageHeight) / 2;
        }
        else
        {
            newHeight = workingTexture.height;
            newWidth = Mathf.RoundToInt(workingTexture.height * targetAspect);
            imageWidth = workingTexture.width;
            imageHeight = workingTexture.height;
            offsetX = (newWidth - imageWidth) / 2;
            offsetY = 0;
        }

        Texture2D letterboxed = new Texture2D(newWidth, newHeight);
        Color[] blackPixels = new Color[newWidth * newHeight];
        for (int i = 0; i < blackPixels.Length; i++)
        {
            blackPixels[i] = Color.black;
        }
        letterboxed.SetPixels(blackPixels);

        Color[] sourcePixels = workingTexture.GetPixels();
        letterboxed.SetPixels(offsetX, offsetY, imageWidth, imageHeight, sourcePixels);
        letterboxed.Apply();

        return letterboxed;
    }

    private Texture2D RotateTexture90(Texture2D source)
    {
        Texture2D rotated = new Texture2D(source.height, source.width);
        Color[] sourcePixels = source.GetPixels();
        Color[] rotatedPixels = new Color[sourcePixels.Length];

        int srcWidth = source.width;
        int srcHeight = source.height;

        for (int x = 0; x < srcWidth; x++)
        {
            for (int y = 0; y < srcHeight; y++)
            {
                int srcIndex = y * srcWidth + x;
                int rotIndex = x * srcHeight + (srcHeight - y - 1);
                rotatedPixels[rotIndex] = sourcePixels[srcIndex];
            }
        }

        rotated.SetPixels(rotatedPixels);
        rotated.Apply();
        return rotated;
    }

    private Texture2D RotateTexture90CCW(Texture2D source)
    {
        Texture2D rotated = new Texture2D(source.height, source.width);
        Color[] sourcePixels = source.GetPixels();
        Color[] rotatedPixels = new Color[sourcePixels.Length];

        int srcWidth = source.width;
        int srcHeight = source.height;

        for (int x = 0; x < srcWidth; x++)
        {
            for (int y = 0; y < srcHeight; y++)
            {
                int srcIndex = y * srcWidth + x;
                int rotIndex = (srcWidth - x - 1) * srcHeight + y;
                rotatedPixels[rotIndex] = sourcePixels[srcIndex];
            }
        }

        rotated.SetPixels(rotatedPixels);
        rotated.Apply();
        return rotated;
    }


    public ProjectData GetCurrentProject()
    {
        return currentProject;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
