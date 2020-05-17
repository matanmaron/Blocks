using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDropHanler : MonoBehaviour
{
    [SerializeField] UIItemSlot cursorSlot = null;
    [SerializeField] GraphicRaycaster m_Raycaster = null;
    [SerializeField] EventSystem m_EventSystem = null;
    
    ItemSlot cursorItemSlot;
    PointerEventData m_PointerEventData;
    
    private void Start()
    {
        cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update()
    {
        if (!World.Instance.inUI)
        {
            return;
        }
        cursorSlot.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            HandleSlotClick(CheckForSlot());
        }
    }

    void HandleSlotClick(UIItemSlot clickedSlot)
    {
        if (clickedSlot == null)
        {
            return;
        }
        if (!cursorSlot.HasItem && !clickedSlot.HasItem)
        {
            return;
        }
        if (clickedSlot.itemSlot.isCreative)
        {
            cursorItemSlot.EmptySlot();
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.stack);
        }
        if (!cursorSlot.HasItem && clickedSlot.HasItem)
        {
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());
            cursorSlot.UpdateSlot();
            return;
        }
        if (cursorSlot.HasItem && !clickedSlot.HasItem)
        {
            clickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
            clickedSlot.UpdateSlot();
            return;
        }
        if (cursorSlot.HasItem && clickedSlot.HasItem)
        {
            if (cursorSlot.itemSlot.stack.id != clickedSlot.itemSlot.stack.id)
            {
                ItemStack oldCursorSlot = cursorSlot.itemSlot.TakeAll();
                ItemStack oldSlot = clickedSlot.itemSlot.TakeAll();
                clickedSlot.itemSlot.InsertStack(oldCursorSlot);
                cursorSlot.itemSlot.InsertStack(oldSlot);
            }
        }
    }

    UIItemSlot CheckForSlot()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;
        m_Raycaster.Raycast(m_PointerEventData, results);
        foreach (var item in results)
        {
            if (item.gameObject.tag == "UIItemSlot")
            {
                return item.gameObject.GetComponent<UIItemSlot>();
            }
        }
        return null;
    }
}
