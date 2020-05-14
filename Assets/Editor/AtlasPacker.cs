using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AtlasPacker : EditorWindow
{
    int blockSize = 16; //in pixels
    int atlasSizeInBlocks = 16;
    int atlasSize;

    Object[] rawTextures = new Object[256];
    List<Texture2D> sortedTextures = new List<Texture2D>();
    Texture2D atlas;

    [MenuItem ("Blocks/Atlas Packer")]
    public static void ShowWindow()
    {
        GetWindow(typeof(AtlasPacker));
    }

    private void OnGUI()
    {
        atlasSize = blockSize * atlasSizeInBlocks;
        GUILayout.Label("Blocks Texture Atlas Packer", EditorStyles.boldLabel);
        blockSize = EditorGUILayout.IntField("Block Size", blockSize);
        atlasSizeInBlocks = EditorGUILayout.IntField("Atlas Size In Blocks", atlasSizeInBlocks);
        GUILayout.Label(atlas);
        if (GUILayout.Button("Load Textures"))
        {
            Debug.Log($"AtlasPacker: Loading...");
            LoadTextures();
            PackAtlas();
        }
        if (GUILayout.Button("Clear Textures"))
        {
            atlas = new Texture2D(atlasSize, atlasSize);
            Debug.Log($"AtlasPacker: Textures Cleared");
        }
        if (GUILayout.Button("Save Atlas"))
        {
            Debug.Log($"AtlasPacker: Saving...");
            byte[] bytes = atlas.EncodeToPNG();
            try
            {
                File.WriteAllBytes(Application.dataPath + "/Textures/Packed_Atlas.png", bytes);
                Debug.Log($"AtlasPacker: SAVED !");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"AtlasPacker: Can't Save Atlas");
                Debug.LogWarning(ex.ToString());
                throw;
            }
        }
    }

    void LoadTextures()
    {
        sortedTextures.Clear();
        rawTextures = Resources.LoadAll("AtlasPacker", typeof(Texture2D));
        int index = 0;
        foreach (Object tex in rawTextures)
        {
            Texture2D t = (Texture2D)tex;
            if (t.width == blockSize && t.height == blockSize)
            {
                sortedTextures.Add(t);
            }
            else
            {
                Debug.Log($"AtlasPacker: file-({tex.name}) Incorrect Size");
            }
            index++;
        }
        Debug.Log($"AtlasPacker: {sortedTextures.Count} Loaded");
    }

    void PackAtlas()
    {
        atlas = new Texture2D(atlasSize, atlasSize);
        Color[] pixels = new Color[atlasSize * atlasSize];
        for (int x = 0; x < atlasSize; x++)
        {
            for (int y = 0; y < atlasSize; y++)
            {
                //get current block
                int currentBlockX = x / blockSize;
                int currentBlockY = y / blockSize;
                int index = currentBlockY * atlasSizeInBlocks + currentBlockX;
                //get pixel in block
                int currentPixelX = x - (currentBlockX * blockSize);
                int currentPixelY = y - (currentBlockY * blockSize);

                if (index < sortedTextures.Count)
                {
                    pixels[(atlasSize - y - 1) * atlasSize + x] = sortedTextures[index].GetPixel(x, blockSize - y - 1);
                }
                else
                {
                    pixels[(atlasSize - y - 1) * atlasSize + x] = new Color(0, 0, 0, 0);
                }
            }
        }
        atlas.SetPixels(pixels);
        atlas.Apply();
    }
}
