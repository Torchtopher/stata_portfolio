using UnityEngine;

[CreateAssetMenu(fileName = "NewProject", menuName = "Project Data Asset", order = 1)]
public class ProjectDataAsset : ScriptableObject
{
    [Header("Project Information")]
    public string projectName;

    [TextArea(4, 10)]
    public string detailedDescription;

    [Header("Visuals")]
    public Texture2D thumbnailTexture;
    public Texture2D[] detailTextures;

    [Header("Settings")]
    public float rotationInterval = 3.0f;

    [Header("Metadata")]
    public string year;
    public string githubLink;

    public ProjectData ToProjectData()
    {
        return new ProjectData
        {
            projectName = this.projectName,
            detailedDescription = this.detailedDescription,
            thumbnailImage = thumbnailTexture != null ? thumbnailTexture.name : "",
            detailImages = GetDetailImageNames(),
            rotationInterval = this.rotationInterval,
            year = this.year,
            githubLink = this.githubLink
        };
    }

    private string[] GetDetailImageNames()
    {
        if (detailTextures == null || detailTextures.Length == 0)
            return new string[0];

        string[] names = new string[detailTextures.Length];
        for (int i = 0; i < detailTextures.Length; i++)
        {
            names[i] = detailTextures[i] != null ? detailTextures[i].name : "";
        }
        return names;
    }
}
