using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoxelState
{
    public byte id;
    [NonSerialized] public ChunkData chunkData;
    [NonSerialized] public VoxelNeighbours neighbours;
    [NonSerialized] public Vector3Int position;

    [NonSerialized] byte _light;

    public byte Light
    {
        get { return _light; }
        set
        {
            if (value != _light)
            {
                byte oldLightValue = _light;
                byte oldCastValue = castLight;
                _light = value;
                if (_light < oldLightValue)
                {
                    List<int> neighboursToDarken = new List<int>();
                    for (int i = 0; i < 6; i++)
                    {
                        if (neighbours[i] != null)
                        {
                            if (neighbours[i].Light <= oldCastValue)
                            {
                                neighboursToDarken.Add(i);
                            }
                            else
                            {
                                neighbours[i].PropogateLight();
                            }
                        }
                    }
                    foreach (var ng in neighboursToDarken)
                    {
                        neighbours[ng].Light = 0;
                    }
                    if (chunkData.chunk != null)
                    {
                        World.Instance.AddChunkToUpdate(chunkData.chunk);
                    }
                }
                else if (_light > 1)  
                {
                    PropogateLight();
                }
            }
        }
    }

    public float LightAsFloat
    {
        get { return Light * VoxelData.UnitOfLight; }
    }

    public byte castLight
    {
        get
        {
            int lightLevel = _light - properties.opacity - 1;
            if (lightLevel < 0)
            {
                lightLevel = 0;
            }
            return (byte)lightLevel;
        }
    }

    public void PropogateLight()
    {
        if (Light < 2)
        {
            return;
        }
        for (int i = 0; i < 6; i++)
        {
            if (neighbours[i] != null)
            {
                if (neighbours[i].Light < castLight)
                {
                    neighbours[i].Light = castLight;
                }
            }
            if (chunkData.chunk != null)
            {
                World.Instance.AddChunkToUpdate(chunkData.chunk);
            }
        }
    }

    public VoxelState(byte _id, ChunkData _chunkData, Vector3Int _pos)
    {
        id = _id;
        chunkData = _chunkData;
        neighbours = new VoxelNeighbours(this);
        position = _pos;
        Light = 0;
    }

    public BlockType properties
    {
        get { return World.Instance.blockTypes[id]; }
    }

    public Vector3Int globalPosition
    {
        get { return new Vector3Int(position.x + chunkData.pos.x, position.y, position.z + chunkData.pos.y); }
    }
}

public class VoxelNeighbours
{
    public readonly VoxelState parent;
    public VoxelState[] _neighbours = new VoxelState[6];
    public VoxelNeighbours (VoxelState _parent)
    {
        parent = _parent;
    }
    public int Length 
    { 
        get { return _neighbours.Length; } 
    }
    public VoxelState this[int index]
    {
        get 
        {
            if (_neighbours[index] == null)
            {
                _neighbours[index] = World.Instance.worldData.GetVoxel(parent.globalPosition + VoxelData.faceChecks[index]);
                ReturnNeighbour(index);
            }
            return _neighbours[index]; 
        }
        set
        {
            _neighbours[index] = value;
            ReturnNeighbour(index);
        }
    }

    void ReturnNeighbour (int index)
    {
        if (_neighbours[index] == null)
        {
            return;
        }
        if (_neighbours[index].neighbours[VoxelData.revFaceCheckIndex[index]] != parent)
        {
            _neighbours[index].neighbours[VoxelData.revFaceCheckIndex[index]] = parent;
        }
    }
}