using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Lode
{
    public string nodeName;
    public BlockTypeEnum BlockType;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}