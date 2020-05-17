using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;
    public static readonly int WorldSizeInChunks = 100;
    //lightning values
    public static float minGlobalLightLevel = 0.1f;
    public static float maxGlobalLightLevel = 0.9f;

    public static int seed;

    public static float UnitOfLight
    {
        get { return 1f / 16f;}
    }
    public static int WorldCenter
    {
        get { return (WorldSizeInChunks * ChunkWidth) / 2; }
    }
    public static int WorldSizeInVoxels
    {
        get { return WorldSizeInChunks * ChunkWidth; }
    }

    public static readonly int TextureAtlasSizeInBlocks = 16;
    public static float NormalizedBlockTextureSize { get { return 1f / (float)TextureAtlasSizeInBlocks; } }

    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3(0f,0f,0f),
        new Vector3(1f,0f,0f),
        new Vector3(1f,1f,0f),
        new Vector3(0f,1f,0f),
        new Vector3(0f,0f,1f),
        new Vector3(1f,0f,1f),
        new Vector3(1f,1f,1f),
        new Vector3(0f,1f,1f)
    };

    public static readonly Vector3Int[] faceChecks = new Vector3Int[6]
    {
        new Vector3Int(0,0,-1),
        new Vector3Int(0,0,1),
        new Vector3Int(0,1,0),
        new Vector3Int(0,-1,0),
        new Vector3Int(-1,0,0),
        new Vector3Int(1,0,0)
    };

    public static readonly int[] revFaceCheckIndex = new int[6] { 1, 0, 3, 2, 5, 4 };

    public static readonly int[,] voxelTris = new int[6, 4]
    {
        //back,front,top,bottom,left,right
        //0,1,2,2,1,3
        {0,3,1,2}, //back face
        {5,6,4,7}, //front face
        {3,7,2,6}, //top face
        {1,5,0,4}, //bottom face
        {4,7,0,3}, //left face
        {1,2,5,6}  //right face
    };

    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2(0,0),
        new Vector2(0,1),
        new Vector2(1,0),
        new Vector2(1,1)
    };
}
