using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.XR.WSA;
using System.IO;
using System;

public class World : MonoBehaviour
{
    [Header("World Generation Values")]
    public BiomAttributes[] biomes;

    [Header("Light")]
    [Range(0,1f)] public float globalLightLevel;
    public Color day;
    public Color night;

    [Header("Other")]
    public Transform player;
    public Vector3 spawnPosition;
    public ChunkCoord playerChunkCoord;
    public Material material;
    public Material transparentMaterial;
    public BlockType[] blockTypes;
    public GameObject DebugScreen;
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();
    public GameObject creativeInventoryWindow;
    public GameObject cursorSlot;
    public object chunkUpdateThreadLock = new object();
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    public Settings settings;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerLastChunkCoord;
    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();
    bool applyingModifications = false;
    bool _inUI = false;
    Thread chunkUpdateThread;
    const int solideGroundHeight = 42;

    public bool inUI
    {
        get { return _inUI; }
        set
        {
            _inUI = value;
            if (_inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                creativeInventoryWindow.SetActive(true);
                cursorSlot.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                creativeInventoryWindow.SetActive(false);
                cursorSlot.SetActive(false);
            }
        }
    }

    private void Start()
    {
        settings.Export(settings);
        settings = settings.Import();
        UnityEngine.Random.InitState(settings.Seed);
        Shader.SetGlobalFloat("MinGlobalLightLevel", VoxelData.minGlobalLightLevel);
        Shader.SetGlobalFloat("MaxGlobalLightLevel", VoxelData.maxGlobalLightLevel);
        if (settings.EnableThreading)
        {
            chunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            chunkUpdateThread.Start();
        }
        SetGlobalLightValue();
        spawnPosition = new Vector3(VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2, VoxelData.ChunkHeight -50, VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2);
        GenerateWorld();
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;
    }

    public void SetGlobalLightValue()
    {
        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLightLevel);
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }
        if (chunksToCreate.Count > 0)
        {
            CreateChunk();
        }
        if (chunksToDraw.Count > 0)
        {
            if (chunksToDraw.Peek().isEditable)
            {
                chunksToDraw.Dequeue().CreateMesh();
            }
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            DebugScreen.SetActive(!DebugScreen.activeSelf);
        }
        if (!settings.EnableThreading)
        {
            if (!applyingModifications)
            {
                ApplyModifications();
            }
            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }
    }


    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks/2) - settings.ViewDistance; x < (VoxelData.WorldSizeInChunks / 2) + settings.ViewDistance; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - settings.ViewDistance; z < (VoxelData.WorldSizeInChunks / 2) + settings.ViewDistance; z++)
            {
                ChunkCoord newChunk = new ChunkCoord(x, z);
                chunks[x, z] = new Chunk(newChunk, this);
                chunksToCreate.Add(newChunk);
            }
        }
        player.position = spawnPosition;
        CheckViewDistance();
    }

    void CreateChunk()
    {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        chunks[c.x, c.z].Init();
    }

    void UpdateChunks()
    {
        bool updated = false;
        int index = 0;
        lock (chunkUpdateThreadLock)
        {
            while (!updated && index < chunksToUpdate.Count - 1)
            {
                if (chunksToUpdate[index].isEditable)
                {
                    chunksToUpdate[index].UpdateChunk();
                    chunksToUpdate.RemoveAt(index);
                    if (!activeChunks.Contains(chunksToUpdate[index].coord))
                    {
                        activeChunks.Add(chunksToUpdate[index].coord);
                    }
                    updated = true;
                }
                else
                {
                    index++;
                }
            }
        }
    }

    void ThreadedUpdate()
    {
        while (true)
        {
            if (!applyingModifications)
            {
                ApplyModifications();
            }
            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }
    }

    private void OnDisable()
    {
        if (settings.EnableThreading)
        {
            chunkUpdateThread.Abort();
        }
    }

    void ApplyModifications()
    {
        applyingModifications = true;
        while (modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();
            while (queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();
                ChunkCoord c = GetChunkCoordFromVector3(v.position);
                if (chunks[c.x, c.z] == null)
                {
                    chunks[c.x, c.z] = new Chunk(c, this);
                    chunksToCreate.Add(c);
                }
                chunks[c.x, c.z].modifications.Enqueue(v);
            }
        }
        applyingModifications = false;
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
        activeChunks.Clear();

        for (int x = coord.x - settings.ViewDistance; x < coord.x + settings.ViewDistance; x++)
        {
            for (int z = coord.z - settings.ViewDistance; z < coord.z + settings.ViewDistance; z++)
            {
                ChunkCoord thisChunkCoord = new ChunkCoord(x, z);
                if (IsChunkInWorld(thisChunkCoord))
                {
                    if (chunks[x,z] == null)
                    {
                        chunks[x, z] = new Chunk(thisChunkCoord, this);
                        chunksToCreate.Add(thisChunkCoord);
                    }
                    else if(!chunks[x,z].isActive)
                    {
                        chunks[x, z].isActive = true;
                        activeChunks.Add(thisChunkCoord);
                    }
                }
                for (int i = 0; i < prevActiveChunks.Count; i++)
                { 
                    if (prevActiveChunks[i].Equal(thisChunkCoord))
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
        if (chunks[thisChunk.x,thisChunk.z] != null && chunks[thisChunk.x,thisChunk.z].isEditable)
        {
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos).id].isSolid;
        }
        return blockTypes[GetVoxel(pos)].isSolid;
    }

    public VoxelState GetVoxelState(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!IsVoxelInWorld(pos))
        {
            return null;
        }
        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
        {
            return chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos);
        }
        return new VoxelState(GetVoxel(pos));
    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        if (!IsVoxelInWorld(pos))
        {//if outside world return air
            return (int)BlockTypeEnum.Air;
        }
        if (yPos == 0)
        {//if bottom return bedrock
            return (int)BlockTypeEnum.Bedrock;
        }

        //biome selection pass
        float sumOfHeights = 0f;
        int count = 0;
        float strongestWeight = 0;
        int strongestWeightIndex = 0;
        for (int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);
            //get srotger biome
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestWeightIndex = i;
            }
            //get terrain height
            float height = biomes[i].terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.y), 0, biomes[i].terrainScale) * weight;
            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }
        }
        //set biome
        BiomAttributes biome = biomes[strongestWeightIndex];

        //get avrage
        sumOfHeights /= count;
        int terrainHeight = Mathf.FloorToInt(sumOfHeights + solideGroundHeight);
        //basic terrain
        byte voxelValue = (int)BlockTypeEnum.Stone;
        if (yPos == terrainHeight)
        {
            voxelValue = (byte)biome.surfaceBlock;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = (byte)biome.subSurfaceBlock;
        }
        else if (yPos > terrainHeight)
        {
            return (int)BlockTypeEnum.Air;
        }
        //second pass
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
        //trees
        if (yPos == terrainHeight && biome.placeMajorFlora)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x,pos.z), 0, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                {
                    modifications.Enqueue(Structure.GenerateMajorFlora(biome.floraType, pos, biome.minHeight, biome.maxHeight));
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
    public bool renderNeighborFaces;
    public float trasparency;
    //stack size??

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

public class VoxelMod
{
    public Vector3 position;
    public byte id;

    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod (Vector3 _pos, byte _id)
    {
        position = _pos;
        id = _id;
    }
}

[System.Serializable]
public class Settings
{
    [Header("Game Data")]
    public string Version;

    [Header("Performance")]
    public int ViewDistance;
    public bool EnableThreading;
    public bool EnableAnimatedChunks;

    [Header("Controls")]
    [Range(0.2f,10f)] public float MouseSensitivity;
    public bool InvertMouseWheel;

    [Header("World Gen")]
    public int Seed;

    internal void Export(Settings settings)
    {
        string jsonExport = JsonUtility.ToJson(settings);
        File.WriteAllText(Application.dataPath + "/settings.dat", jsonExport);
    }

    internal Settings Import()
    {
        string jsonImport = File.ReadAllText(Application.dataPath + "/settings.dat");
        return JsonUtility.FromJson<Settings>(jsonImport);
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
    Cobblestone,
    Glass,
    Leaves,
    Cactus,
    CactusTop
}