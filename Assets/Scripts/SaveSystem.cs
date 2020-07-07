using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;

public static class SaveSystem
{
    public static void SaveWorld (WorldData world)
    {
        return;
        Debug.Log($"SaveSystem - Saving {world.worldName}");
        try
        {
            string savePath = $"{World.Instance.appPath}/saves/{world.worldName}/";
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            string jsonExport = JsonUtility.ToJson(world);
            File.WriteAllText($"{savePath}{world.worldName}.world", jsonExport);
            Thread t = new Thread(() => SaveChunks(world));
            t.Start();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SaveSystem - ERROR {ex}");
        }
    }

    public static void SaveChunks(WorldData world)
    {
        return;
        List<ChunkData> chunks = new List<ChunkData>(world.modifiedChunks);
        world.modifiedChunks.Clear();
        int count = 0;
        foreach (var chunk in chunks)
        {
            SaveChunk(chunk, world.worldName);
            count++;
        }

        Debug.Log($"SaveSystem - {world.worldName} SAVED ! ({count} Chunks");

    }

    public static void SaveChunk(ChunkData chunk, string worldName)
    {
        string chunkName = $"{chunk.pos.x}-{chunk.pos.y}";
        string savePath = $"{World.Instance.appPath}/saves/{worldName}/chunks/";
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        string jsonExport = JsonUtility.ToJson(chunk);
        File.WriteAllText($"{savePath}{chunkName}.chunk", jsonExport);
    }

    public static WorldData LoadWorld(string worldName, int seed = 0)
    {
        return new WorldData(worldName, Random.Range(1,999999));
        string loadPath = World.Instance.appPath + "/saves/" + worldName + "/";
        if (File.Exists($"{loadPath}{worldName}.world"))
        {
            Debug.Log($"Loading {worldName} from file");
            string jsonImport = File.ReadAllText($"{loadPath}/{worldName}.world");
            WorldData world = JsonUtility.FromJson<WorldData>(jsonImport);
            world.chunks = new Dictionary<Vector2Int, ChunkData>();
            world.modifiedChunks = new List<ChunkData>();
            return world;
        }
        Debug.LogWarning($"{worldName} not found. Creating a new world !");
        WorldData world2 = new WorldData(worldName, seed);
        return new WorldData(world2);
    }

    public static ChunkData LoadChunk(string worldName, Vector2Int pos)
    {
        return null;
        string chunkName = $"{pos.x}-{pos.y}";
        string loadPath = $"{World.Instance.appPath}/saves/{worldName}/chunks/{chunkName}.chunk";
        if (File.Exists(loadPath))
        {
            string jsonImport = File.ReadAllText(loadPath);
            ChunkData chunkData = JsonUtility.FromJson<ChunkData>(jsonImport);
            return chunkData;
        }
        return null;
    }
}
