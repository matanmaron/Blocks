using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    TextMeshProUGUI text;
    float frameRate;
    float timer;
    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
        halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;
    }

    void Update()
    {
        StringBuilder debugText = new StringBuilder();
        debugText.AppendLine($"{frameRate} fps");
        debugText.AppendLine($"XYZ: {Mathf.FloorToInt(World.Instance.player.transform.position.x) - halfWorldSizeInVoxels} / {Mathf.FloorToInt(World.Instance.player.transform.position.y)} / {Mathf.FloorToInt(World.Instance.player.transform.position.z) - halfWorldSizeInVoxels}");
        debugText.AppendLine($"Chunk: {World.Instance.playerChunkCoord.x - halfWorldSizeInChunks} / {World.Instance.playerChunkCoord.z - halfWorldSizeInChunks}");

        text.text = debugText.ToString();

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}