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
}
