using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

public static class SaveSystem
{
    public static void SaveWorld (WorldData world)
    {
        Debug.Log($"SaveSystem - Saving {world.worldName}");
        try
        {
            string savePath = World.Instance.appPath + "/saves/" + world.worldName + "/";
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(savePath + "world.world", FileMode.Create);
            formatter.Serialize(stream, world);
            stream.Close();
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
        List<ChunkData> chunks = new List<ChunkData>(world.modifiedChunks);
        world.modifiedChunks.Clear();
        int count = 0;
        foreach (var chunk in chunks)
        {
            if (count == chunks.Count *0.25)
            {
                Debug.Log($"SaveSystem - 25%");
            }
            if (count == chunks.Count * 0.5)
            {
                Debug.Log($"SaveSystem - 50%");
            }
            if (count == chunks.Count *0.75)
            {
                Debug.Log($"SaveSystem - 75%");
            }
            if (count == chunks.Count * 0.9)
            {
                Debug.Log($"SaveSystem - 90%");
            }
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
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream($"{savePath}{chunkName}.chunk", FileMode.Create);
        formatter.Serialize(stream, chunk);
        stream.Close();
    }

    public static WorldData LoadWorld(string worldName, int seed = 0)
    {
        string loadPath = World.Instance.appPath + "/saves/" + worldName + "/";
        if (File.Exists(loadPath + "world.world"))
        {
            Debug.Log($"Loading {worldName} from file");
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "world.world", FileMode.Open);
            WorldData world = formatter.Deserialize(stream) as WorldData;
            stream.Close();
            return world;
        }
        Debug.LogWarning($"{worldName} not found. Creating a new world !");
        WorldData world2 = new WorldData(worldName, seed);
        SaveWorld(world2);
        return new WorldData(world2);
    }

    public static ChunkData LoadChunk(string worldName, Vector2Int pos)
    {
        string chunkName = $"{pos.x}-{pos.y}";
        string loadPath = $"{World.Instance.appPath}/saves/{worldName}/chunks/{chunkName}.chunk";
        if (File.Exists(loadPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath, FileMode.Open);
            ChunkData chunkData = formatter.Deserialize(stream) as ChunkData;
            stream.Close();
            return chunkData;
        }
        return null;
    }
}
