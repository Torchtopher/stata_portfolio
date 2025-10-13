using UnityEngine;

/// <summary>
/// Data structure for project information.
/// Serializable for Unity's JsonUtility.
/// </summary>
[System.Serializable]
public class ProjectData
{
    public string projectName;
    public string detailedDescription;
    public string thumbnailImage;
    public string[] detailImages;
    public float rotationInterval = 3.0f;
    public string year;
    public string githubLink;
}
