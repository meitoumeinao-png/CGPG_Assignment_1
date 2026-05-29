using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class PhysicalTrack : MonoBehaviour
{
    public GameObject trackLinkPrefab;   // your little cube prefab
    public Transform[] pathPoints;       // the empty GameObjects you placed
    public int numberOfLinks = 30;       // how many pieces around the loop
    public float linkSpacing = 0.35f;    // distance between each piece

    private List<GameObject> links = new List<GameObject>();
    private float[] distances;            // how far each link is along the path
    private float totalPathLength;

    void Start()
    {
        // Calculate total length of the path (add up distances between points)
        totalPathLength = 0;
        for (int i = 0; i < pathPoints.Length - 1; i++)
            totalPathLength += Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
        totalPathLength += Vector3.Distance(pathPoints[pathPoints.Length - 1].position, pathPoints[0].position);

        // Spawn all the track links evenly spaced around the loop
        for (int i = 0; i < numberOfLinks; i++)
        {
            GameObject link = Instantiate(trackLinkPrefab, transform);
            links.Add(link);
            // Store the starting distance of this link
            distances[i] = i * linkSpacing;
            // Place it at that distance along the path
            PlaceLinkAtDistance(link, distances[i]);
        }
    }

    void Update()
    {
        // Get how fast the drive sprockets are turning (see Step 4)
        float speed = GetSprocketSpeed();  // we'll write this

        // Move all links forward
        for (int i = 0; i < links.Count; i++)
        {
            distances[i] += speed * Time.deltaTime;
            if (distances[i] > totalPathLength)
                distances[i] -= totalPathLength;
            if (distances[i] < 0)
                distances[i] += totalPathLength;
            PlaceLinkAtDistance(links[i], distances[i]);
        }
    }

    void PlaceLinkAtDistance(GameObject link, float distance)
    {
        // Find which segment of the path this distance falls into
        float accumulated = 0;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            float segLen = Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
            if (distance <= accumulated + segLen)
            {
                float t = (distance - accumulated) / segLen;
                Vector3 pos = Vector3.Lerp(pathPoints[i].position, pathPoints[i + 1].position, t);
                link.transform.position = pos;
                // Make the link face the direction of the path
                Vector3 dir = (pathPoints[i + 1].position - pathPoints[i].position).normalized;
                link.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                return;
            }
            accumulated += segLen;
        }
        // Last segment that wraps to the first point
        float lastSeg = Vector3.Distance(pathPoints[pathPoints.Length - 1].position, pathPoints[0].position);
        float t2 = (distance - accumulated) / lastSeg;
        Vector3 pos2 = Vector3.Lerp(pathPoints[pathPoints.Length - 1].position, pathPoints[0].position, t2);
        Vector3 dir2 = (pathPoints[0].position - pathPoints[pathPoints.Length - 1].position).normalized;
        link.transform.position = pos2;
        link.transform.rotation = Quaternion.LookRotation(dir2, Vector3.up);
    }

    float GetSprocketSpeed()
    {
        // You'll connect this to the drive wheels (next step)
        // For now, return 0.
        return 0;
    }
}