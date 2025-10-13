using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ExclamationPointMeshGenerator : MonoBehaviour
{
    [Header("Mesh Generation Settings")]
    [Tooltip("Height of the vertical bar")]
    public float barHeight = 1.0f;

    [Tooltip("Width of the vertical bar")]
    public float barWidth = 0.2f;

    [Tooltip("Depth of the vertical bar")]
    public float barDepth = 0.2f;

    [Tooltip("Size of the dot at the bottom")]
    public float dotSize = 0.25f;

    [Tooltip("Gap between bar and dot")]
    public float gapSize = 0.15f;

    [Header("Material")]
    [Tooltip("Material to apply to the mesh (leave empty for default)")]
    public Material customMaterial;

    [Tooltip("Color if using default material")]
    public Color color = Color.yellow;

    [Header("Auto-Generate")]
    [Tooltip("Generate mesh on Start")]
    public bool generateOnStart = true;

    [Header("Auto-Position")]
    [Tooltip("Automatically position above parent object")]
    public bool autoPositionAboveParent = true;

    [Tooltip("Height above parent's top")]
    public float heightAboveParent = 0.5f;

    [Tooltip("Offset from parent center in X direction")]
    public float offsetX = 0f;

    [Tooltip("Offset from parent center in Z direction")]
    public float offsetZ = 0f;

    [Tooltip("Update position live when values change in Inspector")]
    public bool liveUpdate = false;

    private float lastHeightAboveParent;
    private float lastOffsetX;
    private float lastOffsetZ;

    private void Start()
    {
        if (autoPositionAboveParent)
        {
            PositionAboveParent();
        }

        if (generateOnStart)
        {
            GenerateExclamationPoint();
        }

        lastHeightAboveParent = heightAboveParent;
        lastOffsetX = offsetX;
        lastOffsetZ = offsetZ;
    }

    private void Update()
    {
        
    }

    private void PositionAboveParent()
    {
        if (transform.parent == null)
        {
            Debug.LogWarning("ExclamationPointMeshGenerator: No parent object found for auto-positioning");
            return;
        }

        Renderer parentRenderer = transform.parent.GetComponent<Renderer>();
        if (parentRenderer != null)
        {
            // calculate where the top of the parent object is in local space
            float topY = parentRenderer.bounds.max.y - transform.parent.position.y;
            Vector3 newLocalPos = new Vector3(offsetX, topY + heightAboveParent, offsetZ);

            transform.localPosition = newLocalPos;

            // want to start the exclamation point above the parent
            InteractionMarker marker = GetComponent<InteractionMarker>();
            if (marker != null)
            {
                marker.UpdateStartPosition(newLocalPos);
            }
        }
        else
        {
            transform.localPosition = new Vector3(0, heightAboveParent, 0);
            Debug.LogWarning("ExclamationPointMeshGenerator: Parent has no Renderer, positioning above pivot point");
        }
    }

    [ContextMenu("Generate Exclamation Point")]
    public void GenerateExclamationPoint()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter == null || meshRenderer == null)
        {
            Debug.LogError("ExclamationPointMeshGenerator: MeshFilter or MeshRenderer not found");
            return;
        }

        Mesh mesh = new Mesh();
        mesh.name = "Exclamation Point";

        // Generate the vertical bar part
        Vector3[] barVertices = CreateBoxVertices(
            Vector3.up * (dotSize + gapSize + barHeight / 2f),
            barWidth,
            barHeight,
            barDepth
        );

        // Generate the dot at the bottom
        Vector3[] dotVertices = CreateBoxVertices(
            Vector3.up * (dotSize / 2f),
            dotSize,
            dotSize,
            dotSize
        );

        // Combine into one mesh
        Vector3[] vertices = new Vector3[barVertices.Length + dotVertices.Length];
        barVertices.CopyTo(vertices, 0);
        dotVertices.CopyTo(vertices, barVertices.Length);

        int[] barTriangles = CreateBoxTriangles(0);
        int[] dotTriangles = CreateBoxTriangles(barVertices.Length);

        int[] triangles = new int[barTriangles.Length + dotTriangles.Length];
        barTriangles.CopyTo(triangles, 0);
        dotTriangles.CopyTo(triangles, barTriangles.Length);

        // Basic normals and UVs - could improve this later for better lighting
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            normals[i] = Vector3.up;
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        if (customMaterial != null)
        {
            meshRenderer.material = customMaterial;
        }
        else
        {
            // Create a shiny default material
            Material defaultMat = new Material(Shader.Find("Standard"));
            defaultMat.color = color;
            defaultMat.SetFloat("_Metallic", 0.2f);
            defaultMat.SetFloat("_Glossiness", 0.8f);
            meshRenderer.material = defaultMat;
        }

        Debug.Log("ExclamationPointMeshGenerator: Generated exclamation point mesh");
    }

    private Vector3[] CreateBoxVertices(Vector3 center, float width, float height, float depth)
    {
        float w = width / 2f;
        float h = height / 2f;
        float d = depth / 2f;

        return new Vector3[]
        {
            center + new Vector3(-w, -h, d),  // 0
            center + new Vector3(w, -h, d),   // 1
            center + new Vector3(w, h, d),    // 2
            center + new Vector3(-w, h, d),   // 3

            center + new Vector3(-w, -h, -d), // 4
            center + new Vector3(w, -h, -d),  // 5
            center + new Vector3(w, h, -d),   // 6
            center + new Vector3(-w, h, -d),  // 7
        };
    }

    private int[] CreateBoxTriangles(int offset = 0)
    {
        return new int[]
        {
            offset + 0, offset + 2, offset + 1,
            offset + 0, offset + 3, offset + 2,

            offset + 5, offset + 6, offset + 4,
            offset + 4, offset + 6, offset + 7,

            offset + 4, offset + 7, offset + 0,
            offset + 0, offset + 7, offset + 3,

            offset + 1, offset + 6, offset + 5,
            offset + 1, offset + 2, offset + 6,

            offset + 3, offset + 6, offset + 2,
            offset + 3, offset + 7, offset + 6,

            offset + 4, offset + 1, offset + 5,
            offset + 4, offset + 0, offset + 1,
        };
    }
}
