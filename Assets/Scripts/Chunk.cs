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
    byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    WorldScript world;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<WorldScript>();
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
        return world.blockTypes[voxelMap[x, y, z]].IsSolid;
    }

    private void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (y < 1)
                    {
                        voxelMap[x, y, z] = (int)BlockTypeEnum.Bedrock;
                    }
                    else if (y == VoxelData.ChunkHeight - 1)
                    {
                        voxelMap[x, y, z] = (int)BlockTypeEnum.Grass;
                    }
                    else
                    {
                        voxelMap[x, y, z] = (int)BlockTypeEnum.Stone;
                    }
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

    void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x,y));
        uvs.Add(new Vector2(x,y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

    private void AddVoxelDataChunk(Vector3 pos)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!CheckVoxel(pos + VoxelData.faceChecks[i]))
            {
                byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i,0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i,1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i,2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[i,3]]);

                AddTexture(world.blockTypes[blockID].GetTextureID(i));

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
