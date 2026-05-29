using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TrackMeshWithRoadWheels : MonoBehaviour
{
    public Transform[] allWheels;   // Assign in order: Front, Road1, Road2, ..., Road6, Back
    public float trackWidth = 0.4f;
    public float trackThickness = 0.1f;

    void Start()
    {
        BuildMesh();
    }

    void BuildMesh()
    {
        // Step 1: Get top and bottom points for every wheel
        Vector3[] topPoints = new Vector3[allWheels.Length];
        Vector3[] bottomPoints = new Vector3[allWheels.Length];

        for (int i = 0; i < allWheels.Length; i++)
        {
            float radius = allWheels[i].localScale.y / 2f;
            Vector3 center = allWheels[i].position;
            topPoints[i] = center + Vector3.up * radius;
            bottomPoints[i] = center + Vector3.down * radius;
        }

        // Step 2: Create the full path in order:
        // topPoints[0] ... topPoints[last] → then bottomPoints[last] ... bottomPoints[0]
        Vector3[] path = new Vector3[allWheels.Length * 2];
        for (int i = 0; i < allWheels.Length; i++)
            path[i] = topPoints[i];
        for (int i = 0; i < allWheels.Length; i++)
            path[allWheels.Length + i] = bottomPoints[allWheels.Length - 1 - i];

        // Step 3: Build the mesh (same extrusion logic as before)
        Vector3[] vertices = new Vector3[path.Length * 2];
        for (int i = 0; i < path.Length; i++)
        {
            Vector3 next = path[(i + 1) % path.Length];
            Vector3 along = (next - path[i]).normalized;
            Vector3 outward = Vector3.Cross(along, Vector3.forward).normalized;

            vertices[i * 2] = path[i] + outward * trackThickness;
            vertices[i * 2 + 1] = path[i] - outward * trackThickness;
        }

        int[] triangles = new int[path.Length * 6];
        for (int i = 0; i < path.Length; i++)
        {
            int next = (i + 1) % path.Length;
            int a = i * 2;
            int b = i * 2 + 1;
            int c = next * 2;
            int d = next * 2 + 1;

            triangles[i * 6 + 0] = a;
            triangles[i * 6 + 1] = c;
            triangles[i * 6 + 2] = b;
            triangles[i * 6 + 3] = c;
            triangles[i * 6 + 4] = d;
            triangles[i * 6 + 5] = b;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}