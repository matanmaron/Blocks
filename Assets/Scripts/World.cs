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
    public BiomeAttributes[] biomes;

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
    public GameObject DebugScreen;
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();
    public GameObject creativeInventoryWindow;
    public GameObject cursorSlot;
    public object chunkUpdateThreadLock = new object();
    public object chunkListThreadLock = new object();
    public Settings settings;
    public Clouds clouds;
    public WorldData worldData;
    public string appPath;
    public BlockType[] blockTypes;

    List<Chunk> chunksToUpdate = new List<Chunk>();
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerLastChunkCoord;
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

    private static World _instance;
    public static World Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        appPath = Application.persistentDataPath;
    }

    private void Start()
    {
        Debug.Log($"Seed id: {VoxelData.seed}");
        worldData = SaveSystem.LoadWorld("Prototype");
        ImportSettings();
        UnityEngine.Random.InitState(VoxelData.seed);
        Shader.SetGlobalFloat("MinGlobalLightLevel", VoxelData.minGlobalLightLevel);
        Shader.SetGlobalFloat("MaxGlobalLightLevel", VoxelData.maxGlobalLightLevel);
        LoadWorld();
        SetGlobalLightValue();
        spawnPosition = new Vector3(VoxelData.WorldCenter, VoxelData.ChunkHeight -50, VoxelData.WorldCenter);
        player.position = spawnPosition;
        CheckViewDistance();
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        if (settings.EnableThreading)
        {
            chunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            chunkUpdateThread.Start();
        }
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
        if (chunksToDraw.Count > 0)
        {
            chunksToDraw.Dequeue().CreateMesh();
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
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveSystem.SaveWorld(worldData);
        }
    }

    void LoadWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - settings.LoadDistance; x < (VoxelData.WorldSizeInChunks / 2) + settings.LoadDistance; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - settings.LoadDistance; z < (VoxelData.WorldSizeInChunks / 2) + settings.LoadDistance; z++)
            {
                worldData.LoadChunk(new Vector2Int(x, z));
            }
        }
    }

    void UpdateChunks()
    {
        lock (chunkUpdateThreadLock)
        {
            chunksToUpdate[0].UpdateChunk();
            if (!activeChunks.Contains(chunksToUpdate[0].coord))
            {
                activeChunks.Add(chunksToUpdate[0].coord);
            }
            chunksToUpdate.RemoveAt(0);
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
                worldData.SetVoxel(v.position, v.id);
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

    public void AddChunkToUpdate(Chunk chunk)
    {
        AddChunkToUpdate(chunk, false);
    }
    public void AddChunkToUpdate(Chunk chunk,bool insert)
    {
        lock (chunkUpdateThreadLock)
        {
            if (!chunksToUpdate.Contains(chunk))
            {
                if (insert)
                {
                    chunksToUpdate.Insert(0, chunk);
                }
                else
                {
                    chunksToUpdate.Add(chunk);
                }
            }
        }
    }
    void CheckViewDistance()
    {
        clouds.UpdateClouds();
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
                        chunks[x, z] = new Chunk(thisChunkCoord);
                    }
                    chunks[x, z].isActive = true;
                    activeChunks.Add(thisChunkCoord);
                }
                for (int i = 0; i < prevActiveChunks.Count; i++)
                { 
                    if (prevActiveChunks[i].Equals(thisChunkCoord))
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
        VoxelState voxel = worldData.GetVoxel(pos);
        if (blockTypes[voxel.id].isSolid)
        {
            return true;
        }
        return false;
    }

    public VoxelState GetVoxelState(Vector3 pos)
    {
        return worldData.GetVoxel(pos);
    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        if (!IsVoxelInWorld(pos))
        {//if outside world return air
            return (byte)BlockTypeEnum.Air;
        }
        if (yPos == 0)
        {//if bottom return bedrock
            return (byte)BlockTypeEnum.Bedrock;
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
        BiomeAttributes biome = biomes[strongestWeightIndex];

        //get avrage
        sumOfHeights /= count;
        int terrainHeight = Mathf.FloorToInt(sumOfHeights + solideGroundHeight);
        //basic terrain
        BlockTypeEnum voxelValue = BlockTypeEnum.Stone;
        if (yPos == terrainHeight)
        {
            voxelValue = biome.surfaceBlock;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = biome.subSurfaceBlock;
        }
        else if (yPos > terrainHeight)
        {
            return (byte)BlockTypeEnum.Air;
        }
        //second pass
        if (voxelValue == BlockTypeEnum.Stone)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.BlockType;
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
        return (byte)voxelValue;
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

    void ImportSettings()
    {
        string jsonImport = File.ReadAllText(Application.dataPath + "/settings.dat");
        settings = JsonUtility.FromJson<Settings>(jsonImport);
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
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
    public byte opacity;

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

    public VoxelMod(Vector3 _pos, BlockTypeEnum _id)
    {
        position = _pos;
        id = (byte)_id;
    }
}

[System.Serializable]
public class Settings
{
    [Header("Game Data")]
    public string Version = "0.0.0.1";

    [Header("Performance")]
    public int LoadDistance = 16;
    public int ViewDistance = 8;
    public bool EnableThreading = true;
    public bool EnableAnimatedChunks = false;
    public CloudStyleEnum clouds = CloudStyleEnum.Fast;

    [Header("Controls")]
    [Range(0.2f,10f)] public float MouseSensitivity = 1f;
    public bool InvertMouseWheel = false;
}