﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        float x = (position.x + 0.1f) / VoxelData.ChunkWidth * scale + offset;
        float y = (position.y + 0.1f) / VoxelData.ChunkWidth * scale + offset;
        return Mathf.PerlinNoise(x, y);
    }

    public static bool Get3DPerlin (Vector3 position, float offset, float scale, float threshold)
    {
        float x = (position.x + offset + 0.1f) * scale;
        float y = (position.y + offset + 0.1f) * scale;
        float z = (position.z + offset + 0.1f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        if ((AB +BC + AC + BA + CB + CA) / 6f > threshold)
        {
            return true;
        }
        return false;
    }
}
