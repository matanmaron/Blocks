﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour
{
    public Player player;
    public RectTransform highlight;
    public ItemSlot[] itemSlots;
    
    World world;
    int slotIndex = 0;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        foreach (var item in itemSlots)
        {
            item.icon.sprite = world.blockTypes[item.itemID].icon;
            item.icon.enabled = true;
        }
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                slotIndex++;
            }
            else
            {
                slotIndex--;
            }
            if (slotIndex > itemSlots.Length - 1)
            {
                slotIndex = 0;
            }
            if (slotIndex < 0)
            {
                slotIndex = itemSlots.Length - 1;
            }
            highlight.position = itemSlots[slotIndex].icon.transform.position;
            player.selectedBlockIndex = itemSlots[slotIndex].itemID;
        }
    }
}

[System.Serializable]
public class ItemSlot
{
    public byte itemID;
    public Image icon;
}