using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbar : MonoBehaviour
{
    public UIItemSlot[] slots;
    public RectTransform highlight;
    public int slotIndex = 0;

    private void Start()
    {
        byte index = 1;
        foreach (UIItemSlot s in slots)
        {
            ItemStack stack = new ItemStack(index, Random.Range(2, 65));
            ItemSlot slot = new ItemSlot(s, stack);
            index++;
        }
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                slotIndex = World.Instance.settings.InvertMouseWheel ? slotIndex - 1 : slotIndex + 1;
            }
            else
            {
                slotIndex = World.Instance.settings.InvertMouseWheel ? slotIndex + 1 : slotIndex - 1;
            }
            if (slotIndex > slots.Length - 1)
            {
                slotIndex = 0;
            }
            if (slotIndex < 0)
            {
                slotIndex = slots.Length - 1;
            }
            highlight.position = slots[slotIndex].slotIcon.transform.position;
        }
    }
}