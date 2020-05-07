using UnityEngine;

public static class VoxelData
{
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

    public static readonly int[,] voxelTris = new int[6, 6]
    {
        {0,3,1,1,3,2}, //back face
        {5,6,4,4,6,7}, //front face
        {3,7,2,2,7,6}, //top face
        {1,5,0,0,5,4}, //bottom face
        {4,7,0,0,7,3}, //left face
        {1,2,5,5,2,6} //right face
    };

    public static readonly Vector2[] voxelUvs = new Vector2[6]
    {
        new Vector2(0,0),
        new Vector2(0,1),
        new Vector2(1,0),
        new Vector2(1,0),
        new Vector2(0,1),
        new Vector2(1,1)
    };
}
