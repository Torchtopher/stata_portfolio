using UnityEngine;
using System.Collections.Generic;

public class PosterDiscoveryTracker : MonoBehaviour
{
    private static PosterDiscoveryTracker instance;
    private static bool isQuitting = false;

    [Header("Discovery Settings")]
    [Tooltip("Enable debug logging for discovery events")]
    public bool enableLogging = false;

    private HashSet<string> discoveredPosters = new HashSet<string>();

    private int totalPosterCount = 0;

    public System.Action<int, int> OnDiscoveryChanged;

    public static PosterDiscoveryTracker Instance
    {
        get
        {
            if (isQuitting)
                return null;

            if (instance == null)
            {
                instance = FindObjectOfType<PosterDiscoveryTracker>();

                if (instance == null && !isQuitting)
                {
                    GameObject go = new GameObject("PosterDiscoveryTracker");
                    instance = go.AddComponent<PosterDiscoveryTracker>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        isQuitting = false;

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CountTotalPosters();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            isQuitting = true;
            instance = null;
        }
    }

    private void CountTotalPosters()
    {
        ProjectDisplayBase[] allPosters = FindObjectsOfType<ProjectDisplayBase>();
        totalPosterCount = allPosters.Length;

        if (enableLogging)
            Debug.Log($"PosterDiscoveryTracker: Found {totalPosterCount} total posters in scene");

        OnDiscoveryChanged?.Invoke(GetDiscoveredCount(), totalPosterCount);
    }

    public bool RegisterDiscovery(string posterId)
    {
        if (string.IsNullOrEmpty(posterId))
        {
            Debug.LogWarning("PosterDiscoveryTracker: Attempted to register discovery with null/empty posterId");
            return false;
        }

        bool isNewDiscovery = discoveredPosters.Add(posterId);

        if (isNewDiscovery)
        {
            if (enableLogging)
                Debug.Log($"PosterDiscoveryTracker: New poster discovered! '{posterId}' ({GetDiscoveredCount()}/{totalPosterCount})");

            OnDiscoveryChanged?.Invoke(GetDiscoveredCount(), totalPosterCount);
        }

        return isNewDiscovery;
    }

    public bool IsDiscovered(string posterId)
    {
        return discoveredPosters.Contains(posterId);
    }

    public int GetDiscoveredCount()
    {
        return discoveredPosters.Count;
    }

    public int GetTotalCount()
    {
        return totalPosterCount;
    }

    public void ResetDiscoveries()
    {
        discoveredPosters.Clear();
        if (enableLogging)
            Debug.Log("PosterDiscoveryTracker: All discoveries reset");

        OnDiscoveryChanged?.Invoke(0, totalPosterCount);
    }

    public void SetTotalCount(int count)
    {
        totalPosterCount = count;
        OnDiscoveryChanged?.Invoke(GetDiscoveredCount(), totalPosterCount);
    }
}
