using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    private void Start()
    {
        int vertexIndex = 0;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triengles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                int triangleIndex = VoxelData.voxelTris[i, j];
                vertices.Add(VoxelData.voxelVerts[triangleIndex]);

                triengles.Add(vertexIndex);

                uvs.Add(VoxelData.voxelUvs[j]);

                vertexIndex++;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triengles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}
