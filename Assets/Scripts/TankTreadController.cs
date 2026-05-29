using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TankTreadController : MonoBehaviour
{
    [Header("Track Path")]
    [Tooltip("Transforms (sprockets, wheels, rollers) in order around the track loop.")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("Tread Settings")]
    public GameObject treadPrefab;      // Cube (or any simple cuboid)
    public int numberOfTreads = 24;     // How many individual tread blocks
    public float treadSpeed = 2f;       // How fast treads rotate when moving

    [Header("Movement (for demo)")]
    public float tankSpeed = 3f;        // Speed of the whole tank (forward/back)
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;

    // Internal data
    private List<GameObject> treads = new List<GameObject>();
    private List<float> segmentLengths = new List<float>();
    private List<Vector3> segmentDirections = new List<Vector3>();
    private float totalPathLength;
    private float treadOffset = 0f;

    void Start()
    {
        if (waypoints.Count < 2)
        {
            Debug.LogError("Need at least 2 waypoints for the track path.");
            return;
        }
        ComputePathData();
        CreateTreads();
    }

    void Update()
    {
        // Simple tank movement (move this whole GameObject forward/back)
        float move = 0f;
        if (Input.GetKey(forwardKey)) move = 1f;
        else if (Input.GetKey(backwardKey)) move = -1f;

        Vector3 movement = transform.forward * move * tankSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // Update tread offset based on movement (forward/backward)
        treadOffset += move * treadSpeed * Time.deltaTime;
        if (treadOffset > totalPathLength) treadOffset -= totalPathLength;
        else if (treadOffset < 0) treadOffset += totalPathLength;

        // Reposition each tread block along the path
        for (int i = 0; i < treads.Count; i++)
        {
            float distance = (treadOffset + (i * totalPathLength / numberOfTreads)) % totalPathLength;
            Vector3 pos;
            Quaternion rot;
            GetPointOnPath(distance, out pos, out rot);
            treads[i].transform.position = pos;
            treads[i].transform.rotation = rot;
        }
    }

    // ------------------------------------------------------------
    // Build the path data (linear segments between waypoints)
    // ------------------------------------------------------------
    void ComputePathData()
    {
        segmentLengths.Clear();
        segmentDirections.Clear();
        totalPathLength = 0f;

        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 a = waypoints[i].position;
            Vector3 b = waypoints[(i + 1) % waypoints.Count].position;
            float segLen = Vector3.Distance(a, b);
            segmentLengths.Add(segLen);
            segmentDirections.Add((b - a).normalized);
            totalPathLength += segLen;
        }
    }

    // Get position and rotation at a given distance along the closed path
    void GetPointOnPath(float distance, out Vector3 position, out Quaternion rotation)
    {
        float accumulated = 0f;
        for (int i = 0; i < segmentLengths.Count; i++)
        {
            float segLen = segmentLengths[i];
            if (distance <= accumulated + segLen)
            {
                float t = (distance - accumulated) / segLen;
                Vector3 start = waypoints[i].position;
                Vector3 end = waypoints[(i + 1) % waypoints.Count].position;
                position = Vector3.Lerp(start, end, t);
                rotation = Quaternion.LookRotation(segmentDirections[i], transform.up);
                return;
            }
            accumulated += segLen;
        }
        // Fallback (should never happen)
        position = waypoints[0].position;
        rotation = Quaternion.identity;
    }

    // Create or refresh the tread blocks
    void CreateTreads()
    {
        // Remove old treads
        foreach (var tread in treads)
            if (tread != null) Destroy(tread);
        treads.Clear();

        if (treadPrefab == null)
        {
            Debug.LogError("Tread Prefab not assigned.");
            return;
        }

        // Instantiate new treads at even distances along the path
        for (int i = 0; i < numberOfTreads; i++)
        {
            float distance = i * totalPathLength / numberOfTreads;
            Vector3 pos;
            Quaternion rot;
            GetPointOnPath(distance, out pos, out rot);
            GameObject tread = Instantiate(treadPrefab, pos, rot, transform);
            treads.Add(tread);
        }
    }

    // Editor helper: press "R" while in Play Mode to rebuild treads
    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Count < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 a = waypoints[i].position;
            Vector3 b = waypoints[(i + 1) % waypoints.Count].position;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawSphere(a, 0.05f);
        }
    }
}
