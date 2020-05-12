using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour
{
    public bool isLinked = false;
    public ItemSlot itemSlot;
    public Image slotImage;
    public Image slotIcon;
    public TextMeshProUGUI slotAmount;

    World world;

    private void Awake()
    {
        world = GameObject.Find("World").GetComponent<World>();
    }

    public bool HasItem
    {
        get
        {
            if (itemSlot == null)
            {
                return false;
            }
            return itemSlot.HasItem;
        }
    }

    public void Link(ItemSlot _itemSlot)
    {
        itemSlot = _itemSlot;
        isLinked = true;
        itemSlot.LinkUISlot(this);
        UpdateSlot();
    }

    public void UnLink()
    {
        itemSlot.UnLinkUISlot();
        itemSlot = null;
        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (itemSlot != null && itemSlot.HasItem)
        {
            slotIcon.sprite = world.blockTypes[itemSlot.stack.id].icon;
            slotAmount.text = itemSlot.stack.amout.ToString();
            slotIcon.enabled = true;
            slotAmount.enabled = true;
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        slotIcon.sprite = null;
        slotAmount.text = string.Empty;
        slotIcon.enabled = false;
        slotAmount.enabled = false;
    }

    private void OnDestroy()
    {
        if (isLinked)
        {
            itemSlot.UnLinkUISlot();
        }
    }
}

public class ItemSlot
{
    public ItemStack stack = null;

    UIItemSlot uIItemSlot = null;

    public ItemSlot(UIItemSlot _uIItemSlot)
    {
        stack = null;
        uIItemSlot = _uIItemSlot;
        uIItemSlot.Link(this);
    }

    public ItemSlot(UIItemSlot _uIItemSlot, ItemStack _stack)
    {
        stack = _stack;
        uIItemSlot = _uIItemSlot;
        uIItemSlot.Link(this);
    }

    public bool HasItem
    {
        get
        {
            if (stack != null)
            {
                return true;
            }
            return false;
        }
    }

    public void LinkUISlot(UIItemSlot uiSlot)
    {
        uIItemSlot = uiSlot;
    }

    public void UnLinkUISlot()
    {
        uIItemSlot = null;
    }

    public void EmptySlot()
    {
        stack = null;
        if (uIItemSlot != null)
        {
            uIItemSlot.UpdateSlot();
        }
    }

    public int Take(int amt)
    {
        if (amt > stack.amout)
        {
            var res = stack.amout;
            EmptySlot();
            return res;
        }
        else if (amt < stack.amout)
        {
            stack.amout -= amt;
            uIItemSlot.UpdateSlot();
            return amt;
        }
        EmptySlot();
        return amt;
    }
}