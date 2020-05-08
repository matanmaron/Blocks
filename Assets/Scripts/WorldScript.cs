using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldScript : MonoBehaviour
{
    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public BlockType[] blockTypes;
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    private void Start()
    {
        spawnPosition = new Vector3(VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2, VoxelData.ChunkHeight + 2, VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2);
        GenerateWorld();
        playerLastChunkCoord = GetChunkFromVector3(player.position);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkFromVector3(player.position);
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }
    }

    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks/2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                CreateNewChunk(x, z);
            }
        }
        player.position = spawnPosition;
    }

    ChunkCoord GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkFromVector3(player.position);
        List<ChunkCoord> prevActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                if (IsChunkInWorld(new ChunkCoord(x,z)))
                {
                    if (chunks[x,z] == null)
                    {
                        CreateNewChunk(x, z);
                    }
                    else if(!chunks[x,z].isActive)
                    {
                        chunks[x, z].isActive = true;
                        activeChunks.Add(new ChunkCoord(x, z));
                    }
                }
                for (int i = 0; i < prevActiveChunks.Count; i++)
                { 
                    if (prevActiveChunks[i].Equal(new ChunkCoord(x,z)))
                    {
                        prevActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach (var c in prevActiveChunks)
        {
            chunks[c.x, c.z].isActive = false;
            //activeChunks.re
        }
    }

    public byte GetVoxel(Vector3 pos)
    {
        if (!IsVoxelInWorld(pos))
        {
            return 0;
        }
        if (pos.y < 1)
        {
            return (int)BlockTypeEnum.Bedrock;
        }
        else if (pos.y == VoxelData.ChunkHeight - 1)
        {
           return (int)BlockTypeEnum.Grass;
        }
        else
        {
            return (int)BlockTypeEnum.Stone;
        }
    }

    private void CreateNewChunk(int x, int z)
    {
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        activeChunks.Add(new ChunkCoord(x, z));
    }

    bool IsChunkInWorld (ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1)
        {
            if (coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
            {
                return true;
            }
        }
        return false;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels)
        {
            if (pos.y >= 0 && pos.y < VoxelData.ChunkHeight)
            {
                if (pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

[System.Serializable]
public class BlockType
{
    public string BlockName;
    public bool IsSolid;

    [Header("Texture Values")]
    //back,front,top,bottom,left,right
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    public int GetTextureID (int faceIndex)
    {
        switch (faceIndex)
        {
            case 0: return backFaceTexture;
            case 1: return frontFaceTexture;
            case 2: return topFaceTexture;
            case 3: return bottomFaceTexture;
            case 4: return leftFaceTexture;
            case 5: return rightFaceTexture;
            default:
                Debug.LogError($"Error in GetTextureID, index {faceIndex}");
                return 0;
        }
    }
}

public enum BlockTypeEnum
{
    Air,
    Bedrock,
    Stone,
    Grass,
    Furnace,
}