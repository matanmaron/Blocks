using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack
{
    public byte id;
    public int amout;

    public ItemStack(byte _id, int _amount)
    {
        id = _id;
        amout = _amount;
    }
}
