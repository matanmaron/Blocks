using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA;

public class World : MonoBehaviour
{
    public int seed;
    public BiomAttributes biome;
    public Transform player;
    public Vector3 spawnPosition;
    public ChunkCoord playerChunkCoord;
    public Material material;
    public BlockType[] blockTypes;
    public GameObject DebugScreen;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerLastChunkCoord;
    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    bool isCreatingChunks;

    private void Start()
    {
        Random.InitState(seed);

        spawnPosition = new Vector3(VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2, VoxelData.ChunkHeight -50, VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2);
        GenerateWorld();
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }
        if (chunksToCreate.Count > 0 && !isCreatingChunks)
        {
            StartCoroutine("CreateChunks");
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            DebugScreen.SetActive(!DebugScreen.activeSelf);
        }
    }

    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks/2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }
        player.position = spawnPosition;
    }

    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;
        while (chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }
        isCreatingChunks = false;
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return chunks[x, z];
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        List<ChunkCoord> prevActiveChunks = new List<ChunkCoord>(activeChunks);
        playerLastChunkCoord = playerChunkCoord;

        for (int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                if (IsChunkInWorld(new ChunkCoord(x,z)))
                {
                    if (chunks[x,z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
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
        }
    }

    public bool CheckForVoxel(float _x, float _y, float _z)
    {
        return CheckForVoxel(new Vector3(_x, _y, _z));
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!IsVoxelInWorld(pos))
        {
            return false;
        }
        if (chunks[thisChunk.x,thisChunk.z] != null && chunks[thisChunk.x,thisChunk.z].isVoxelMapPopulated)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isSolid;
        }
        return blockTypes[GetVoxel(pos)].isSolid;
    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        if (!IsVoxelInWorld(pos))
        {
            return (int)BlockTypeEnum.Air;
        }
        if (yPos == 0)
        {
            return (int)BlockTypeEnum.Bedrock;
        }

        float perlin = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale);
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * perlin) + biome.solideGroundHeight;
        byte voxelValue = 0;

        if (yPos == terrainHeight)
        {
            voxelValue = (int)BlockTypeEnum.Grass;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = (int)BlockTypeEnum.Dirt;
        }
        else if (yPos > terrainHeight)
        {
            return (int)BlockTypeEnum.Air;
        }
        else
        {
            voxelValue = (int)BlockTypeEnum.Stone;
        }

        if (voxelValue == (int)BlockTypeEnum.Stone)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }
        return voxelValue;
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
    public string blockName;
    public bool isSolid;
    public Sprite icon;

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
    Sand,
    Dirt,
    Wood,
    Planks,
    Brick,
    Cobblestone
}