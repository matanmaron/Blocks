using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Blocks/Biome Attributes")]
public class BiomAttributes : ScriptableObject
{
    public string biomeName;
    public int solideGroundHeight;
    public int terrainHeight;
    public float terrainScale;

    public Lode[] lodes;
}

[System.Serializable]
public class Lode
{
    public string nodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}