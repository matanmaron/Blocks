using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    public int cloudHeight = 100;
    
    [SerializeField] Texture2D cloudPattern = null;
    [SerializeField] Material cloudMaterial = null;
    [SerializeField] World world = null;

    bool[,] cloudData;
    int cloudTexWidth;
    int cloudTileSize;
    Vector3Int offset;
    Dictionary<Vector2Int, GameObject> clouds = new Dictionary<Vector2Int, GameObject>();

    private void Start()
    {
        cloudTexWidth = cloudPattern.width;
        cloudTileSize = VoxelData.ChunkWidth;
        offset = new Vector3Int(-cloudTexWidth/2, 0, -cloudTexWidth/2);
        transform.position = new Vector3(VoxelData.WorldCenter, cloudHeight, VoxelData.WorldCenter);
        LoadCloudData();
        CreateClouds();
    }

    void LoadCloudData()
    {
        cloudData = new bool[cloudTexWidth, cloudTexWidth];
        Color[] cloudTex = cloudPattern.GetPixels();

        for (int x = 0; x < cloudTexWidth; x++)
        {
            for (int y = 0; y < cloudTexWidth; y++)
            {
                cloudData[x, y] = (cloudTex[y * cloudTexWidth + x].a > 0);
            }
        }
    }

    void CreateClouds()
    {
        for (int x = 0; x < cloudTexWidth; x+=cloudTileSize)
        {
            for (int z = 0; z < cloudTexWidth; z+=cloudTileSize)
            {
                Vector3 pos = new Vector3(x, cloudHeight, z);
                clouds.Add(CloudTilePosFromV3(pos), CreateCloudTile(CreateCloudMesh(x, z), pos));
            }
        }
    }

    public void UpdateClouds()
    {
        for (int x = 0; x < cloudTexWidth; x += cloudTileSize)
        {
            for (int z = 0; z < cloudTexWidth; z += cloudTileSize)
            {
                Vector3 pos = world.player.position + new Vector3(x, 0, z) + offset;
                pos = new Vector3(RoundToCloud(pos.x), cloudHeight, RoundToCloud(pos.z));
                Vector2Int cloudPos = CloudTilePosFromV3(pos);
                clouds[cloudPos].transform.position = pos;
            }
        }
    }

    int RoundToCloud(float val)
    {
        return Mathf.FloorToInt(val / cloudTileSize) * cloudTileSize;
    }

    Mesh CreateCloudMesh(int x, int z)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;
        for (int xIncrement = 0; xIncrement < cloudTileSize; xIncrement++)
        {
            for (int zIncrement = 0; zIncrement < cloudTileSize; zIncrement++)
            {
                int xVal = x + xIncrement;
                int zVal = z + zIncrement;

                if (cloudData[xVal, zVal])
                {
                    vertices.Add(new Vector3(xIncrement, 0, zIncrement));
                    vertices.Add(new Vector3(xIncrement, 0, zIncrement + 1));
                    vertices.Add(new Vector3(xIncrement + 1, 0, zIncrement + 1));
                    vertices.Add(new Vector3(xIncrement + 1, 0, zIncrement));

                    for (int i = 0; i < 4; i++)
                    {
                        normals.Add(Vector3.down);
                    }

                    triangles.Add(vertCount + 1);
                    triangles.Add(vertCount);
                    triangles.Add(vertCount + 2);

                    triangles.Add(vertCount + 2);
                    triangles.Add(vertCount);
                    triangles.Add(vertCount + 3);

                    vertCount += 4;
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;
    }

    GameObject CreateCloudTile(Mesh mesh, Vector3 pos)
    {
        GameObject newCloudTile = new GameObject();
        newCloudTile.transform.position = pos;
        newCloudTile.transform.parent = transform;
        newCloudTile.name = $"Cloud {pos.x},{pos.z}";
        MeshFilter mf = newCloudTile.AddComponent<MeshFilter>();
        MeshRenderer mr = newCloudTile.AddComponent<MeshRenderer>();
        mr.material = cloudMaterial;
        mf.mesh = mesh;
        return newCloudTile;
    }

    Vector2Int CloudTilePosFromV3(Vector3 pos)
    {
        return new Vector2Int(CloudTileCoordFromFloat(pos.x), CloudTileCoordFromFloat(pos.z));
    }

    int CloudTileCoordFromFloat(float val)
    {
        float a = val / cloudTexWidth;
        a -= Mathf.FloorToInt(a);
        return Mathf.FloorToInt(cloudTexWidth * a);
    }
}
