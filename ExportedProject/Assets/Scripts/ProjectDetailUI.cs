using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ProjectDetailUI : MonoBehaviour
{
    [Header("UI Layout")]
    public Canvas canvas;
    public GameObject mainPanel;
    public Button closeButton;
    
    [Header("Text Elements (Left Side)")]
    public TextMeshProUGUI projectTitleText;
    public TextMeshProUGUI detailedDescriptionText;
    public TextMeshProUGUI yearText;
    public TextMeshProUGUI githubLinkText;

    [Header("UI Hints")]
    public TextMeshProUGUI escHintText;
    public TextMeshProUGUI arrowKeyHintText;

    [Header("Image Display (Right Side)")]
    public Transform imageContainer;
    public GameObject imagePrefab;
    public ScrollRect imageScrollRect;

    [Header("Settings")]
    public KeyCode closeKey = KeyCode.Q;
    public float fadeSpeed = 5.0f;
    public float imageRotationInterval = 3.0f;
    public KeyCode nextImageKey = KeyCode.RightArrow;
    public KeyCode prevImageKey = KeyCode.LeftArrow;

    private CanvasGroup canvasGroup;
    private ProjectData currentProject;
    private Texture2D[] currentTextures;
    private Texture2D[] cachedLetterboxedTextures;
    private bool isVisible = false;
    private int currentImageIndex = 0;
    private float lastImageRotationTime;
    private Image displayImage;
    
    private void Awake()
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        if (detailedDescriptionText != null)
        {
            detailedDescriptionText.raycastTarget = true;
        }

        SetVisible(false);
    }
    
    private void Update()
    {
        if (!isVisible) return;

        if (Input.GetKeyDown(closeKey))
        {
            Hide();
        }

        if (Input.GetKeyDown(nextImageKey))
        {
            NextImage();
        }
        else if (Input.GetKeyDown(prevImageKey))
        {
            PreviousImage();
        }

        if (currentTextures != null && currentTextures.Length > 1)
        {
            float interval = currentProject?.rotationInterval ?? imageRotationInterval;
            if (Time.time - lastImageRotationTime >= interval)
            {
                NextImage();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            CheckLinkClick(detailedDescriptionText);
            CheckLinkClick(githubLinkText);
        }
    }

    private void CheckLinkClick(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, Input.mousePosition, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
            string url = linkInfo.GetLinkID();
            Application.OpenURL(url);
            Debug.Log($"Opened link: {url}");
        }
    }
    
    public void Show(ProjectData projectData, Texture2D[] displayTextures)
    {
        Debug.Log($"Show called - imagePrefab before PopulateImages: {imagePrefab != null}");
        currentProject = projectData;
        currentTextures = displayTextures;
        currentImageIndex = 0;
        lastImageRotationTime = Time.time;

        if (displayTextures != null)
        {
            cachedLetterboxedTextures = new Texture2D[displayTextures.Length];
        }

        PopulateProjectInfo();
        SetupImageDisplay();
        SetVisible(true);
        Debug.Log($"Show complete - imagePrefab after PopulateImages: {imagePrefab != null}");
    }
    
    public void Hide()
    {
        Debug.Log($"Hide called - imagePrefab before hide: {imagePrefab != null}");
        SetVisible(false);

        ControlsDisplayUI controlsDisplay = FindObjectOfType<ControlsDisplayUI>();
        if (controlsDisplay != null)
        {
            controlsDisplay.Show();
        }

        Debug.Log($"Hide complete - imagePrefab after hide: {imagePrefab != null}");
    }
    
    private void SetVisible(bool visible)
    {
        isVisible = visible;

        if (canvas != null)
            canvas.enabled = visible;

        if (mainPanel != null)
            mainPanel.SetActive(visible);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        if (escHintText != null)
        {
            escHintText.gameObject.SetActive(visible);
            escHintText.text = "Q to close";
        }
        if (arrowKeyHintText != null)
        {
            arrowKeyHintText.gameObject.SetActive(visible);
            arrowKeyHintText.text = "Left/Right arrow keys to cycle images";
        }

        GroundController groundController = FindObjectOfType<GroundController>();
        FreeCamController freeCamController = FindObjectOfType<FreeCamController>();

        if (groundController != null)
            groundController.enabled = !visible;

        if (freeCamController != null)
            freeCamController.enabled = !visible;

        if (visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    private void PopulateProjectInfo()
    {
        if (currentProject == null) return;

        if (projectTitleText != null)
            projectTitleText.text = currentProject.projectName ?? "Untitled Project";

        if (detailedDescriptionText != null)
            detailedDescriptionText.text = currentProject.detailedDescription ?? "";

        if (yearText != null)
            yearText.text = currentProject.year ?? "";

        if (githubLinkText != null)
        {
            if (!string.IsNullOrEmpty(currentProject.githubLink))
            {
                githubLinkText.text = $"<link=\"{currentProject.githubLink}\"><color=#00BFFF><u>GitHub (clickable!)</u></color></link>";
                githubLinkText.gameObject.SetActive(true);
                githubLinkText.raycastTarget = true;
            }
            else
            {
                githubLinkText.gameObject.SetActive(false);
            }
        }
    }
    
    private void SetupImageDisplay()
    {
        Debug.Log($"SetupImageDisplay called - imageContainer: {imageContainer != null}, imagePrefab: {imagePrefab != null}, textures: {currentTextures != null}, texture count: {currentTextures?.Length ?? 0}");

        if (imageContainer == null)
        {
            Debug.LogError("SetupImageDisplay: imageContainer is null!");
            return;
        }

        if (imagePrefab == null)
        {
            Debug.LogError("SetupImageDisplay: imagePrefab is null!");
            return;
        }

        foreach (Transform child in imageContainer)
        {
            if (child.gameObject != imagePrefab)
            {
                Destroy(child.gameObject);
            }
        }

        if (currentTextures == null || currentTextures.Length == 0)
        {
            Debug.LogWarning("SetupImageDisplay: No textures to display!");
            return;
        }

        GameObject imageObj = Instantiate(imagePrefab, imageContainer);
        imageObj.SetActive(true);
        imageObj.name = "CyclingProjectImage";

        displayImage = imageObj.GetComponent<Image>();
        if (displayImage != null)
        {
            displayImage.enabled = true;

            if (displayImage.material == null)
            {
                displayImage.material = new Material(Shader.Find("UI/Default"));
            }
            displayImage.color = Color.white;

            ShowCurrentImage();
        }
        else
        {
            Debug.LogError("SetupImageDisplay: Image component not found on prefab!");
        }
    }

    private void ShowCurrentImage()
    {
        if (displayImage == null || currentTextures == null || currentTextures.Length == 0)
            return;

        Texture2D texture = currentTextures[currentImageIndex];
        if (texture != null)
        {
            RectTransform imageRect = displayImage.GetComponent<RectTransform>();
            float targetAspect = imageRect.rect.width / imageRect.rect.height;

            float sourceAspect = (float)texture.width / texture.height;

            Texture2D finalTexture = texture;

            if (cachedLetterboxedTextures[currentImageIndex] != null)
            {
                finalTexture = cachedLetterboxedTextures[currentImageIndex];
            }
            else
            {
                bool needsExifRotation = currentProject != null &&
                                         currentProject.detailImages != null &&
                                         currentImageIndex < currentProject.detailImages.Length &&
                                         currentProject.detailImages[currentImageIndex] != null &&
                                         currentProject.detailImages[currentImageIndex] == "yarf_side_view.jpg";

                finalTexture = CreateLetterboxedTexture(texture, targetAspect, needsExifRotation);

                cachedLetterboxedTextures[currentImageIndex] = finalTexture;

                currentTextures[currentImageIndex] = finalTexture;
            }

            Sprite sprite = Sprite.Create(finalTexture, new Rect(0, 0, finalTexture.width, finalTexture.height), new Vector2(0.5f, 0.5f));
            displayImage.sprite = sprite;
            Debug.Log($"Showing image {currentImageIndex + 1}/{currentTextures.Length} ({finalTexture.width}x{finalTexture.height})");
        }
    }

    private void NextImage()
    {
        if (currentTextures == null || currentTextures.Length <= 1) return;

        currentImageIndex = (currentImageIndex + 1) % currentTextures.Length;
        ShowCurrentImage();
        lastImageRotationTime = Time.time;
    }

    private void PreviousImage()
    {
        if (currentTextures == null || currentTextures.Length <= 1) return;

        currentImageIndex--;
        if (currentImageIndex < 0)
            currentImageIndex = currentTextures.Length - 1;

        ShowCurrentImage();
        lastImageRotationTime = Time.time;
    }
    
    public void SetCloseKey(KeyCode key)
    {
        closeKey = key;
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
}