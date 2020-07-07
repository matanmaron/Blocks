using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChunkData
{
    [NonSerialized] public VoxelState[,,] map = new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    [NonSerialized] public Chunk chunk;

    public int x;
    public int y;

    public Vector2Int pos
    {
        get { return new Vector2Int(x, y); }
        set
        {
            x = value.x;
            y = value.y;
        }
    }

    public ChunkData(Vector2Int _pos)
    {
        pos = _pos;
    }

    public ChunkData(int _x, int _y)
    {
        pos = new Vector2Int(_x, _y);
    }

    public void Populate()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    Vector3 voxelGlobalPos = new Vector3(x + pos.x, y, z + pos.y);
                    map[x, y, z] = new VoxelState(World.Instance.GetVoxel(voxelGlobalPos), this, new Vector3Int(x,y,z));
                    for (int i = 0; i < 6; i++)
                    {
                        var face = VoxelData.faceChecks[i];
                        Vector3Int nighbourV3 = new Vector3Int(x, y, z) + face;
                        if (IsVoxelInChunk(nighbourV3))
                        {
                            map[x, y, z].neighbours[i] = VoxelFromV3Int(nighbourV3);
                        }
                        else
                        {
                            map[x, y, z].neighbours[i] = World.Instance.worldData.GetVoxel(voxelGlobalPos + face);
                        }
                    }
                }
            }
        }
        Lighting.RecalculateNaturalLight(this);
        World.Instance.worldData.AddToModifiedChunkList(this);
    }

    public void ModifyVoxel (Vector3Int pos, byte _id)
    {
        if (map[pos.x,pos.y,pos.z].id == _id)
        {
            return;
        }
        VoxelState voxel = map[pos.x, pos.y, pos.z];
        //BlockType newVoxel = World.Instance.blockTypes[_id];
        byte oldOpacity = voxel.properties.opacity;
        voxel.id = _id;
        if (voxel.properties.opacity != oldOpacity && 
            (pos.y == VoxelData.ChunkHeight - 1 || map[pos.x, pos.y + 1, pos.z].Light == 15))
        {
            Lighting.CastNaturalLight(this, pos.x, pos.z, pos.y + 1);
        }
        World.Instance.worldData.AddToModifiedChunkList(this);
        if (chunk != null)
        {
            World.Instance.AddChunkToUpdate(chunk);
        }
    }

    public bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1)
        {
            return false;
        }
        if (y < 0 || y > VoxelData.ChunkHeight - 1)
        {
            return false;
        }
        if (z < 0 || z > VoxelData.ChunkWidth - 1)
        {
            return false;
        }
        return true;
    }

    public bool IsVoxelInChunk(Vector3Int pos)
    {
        return IsVoxelInChunk(pos.x, pos.y, pos.z);
    }

    public VoxelState VoxelFromV3Int(Vector3Int pos)
    {
        return map[pos.x, pos.y, pos.z];
    }
}