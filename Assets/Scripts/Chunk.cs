using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triengles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    bool[,,] voxelMap = new bool[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    private void Start()
    {
        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }

    private void CreateMeshData()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    AddVoxelDataChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    private bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x<0 || x>VoxelData.ChunkWidth - 1)
        {
            return false;
        }
        if (y < 0 || y > VoxelData.ChunkHeight - 1)
        {
            return false;
        }
        if (z < 0 || z > VoxelData.ChunkWidth - 1)
        {
            return false;
        }
        return voxelMap[x, y, z];
    }

    private void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = true;
                }
            }
        }
    }

    private void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triengles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    private void AddVoxelDataChunk(Vector3 pos)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!CheckVoxel(pos + VoxelData.faceChecks[i]))
            {
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i,0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i,1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i,2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i,3]]);
                uvs.Add(VoxelData.voxelUvs[0]);
                uvs.Add(VoxelData.voxelUvs[1]);
                uvs.Add(VoxelData.voxelUvs[2]);
                uvs.Add(VoxelData.voxelUvs[3]);
                triengles.Add(vertexIndex);
                triengles.Add(vertexIndex + 1);
                triengles.Add(vertexIndex + 2);
                triengles.Add(vertexIndex + 2);
                triengles.Add(vertexIndex + 1);
                triengles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }
}
