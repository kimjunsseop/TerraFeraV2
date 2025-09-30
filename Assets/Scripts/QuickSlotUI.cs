using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class QuickSlotUI : MonoBehaviour, IDropHandler
{
    public Image icon;            // 아이템 아이콘 이미지
    public ItemData itemData;      // 슬롯에 등록된 아이템 데이터
    public TextMeshPro quantityText;
    public int quantity;
    public int linkedSlotIndex = -1;
    
    // 아이템 슬롯에 아이템을 등록하는 함수
    private void Awake()
    {
        if (icon == null)
        {
            icon = GetComponentInChildren<UnityEngine.UI.Image>(true); // 하위 오브젝트에서 Image 자동 검색
        }

        if (icon == null)
        {
            Debug.LogError("[QuickSlotUI] icon 이미지 컴포넌트가 연결되어 있지 않습니다!");
        }    
    }
    private void Start()
    {
        ClearSlot();
    }
    public void SetItem(ItemData newItem, Sprite newIcon, int newQuantity = 0)
    {
        if (newItem == null)
        {
            return;
        }
        if (icon == null)
        {
            Debug.LogError("QuickSlotUI: icon 연결 안 됨");
        }
        itemData = newItem;
        quantity = newQuantity;
        if (newIcon != null)
        {
            icon.sprite = newIcon;
            icon.enabled = true;
            icon.color = new Color(1,1,1,1);
        }
        if(quantityText != null)
        {
            quantityText.text = newQuantity.ToString();
            quantityText.enabled = true;
        }
    }
    // 슬롯을 비우는 함수
    public void ClearSlot()
    {
        itemData = null;
        quantity = 0;
        if(icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
        if(quantityText != null)
        {
            quantityText.text = "";
            quantityText.enabled = false;
        }
    }

    // 드랍될 때 호출되는 함수
    public void OnDrop(PointerEventData eventData)
    {
        ItemSlotUI draggedSlot = eventData.pointerDrag?.GetComponent<ItemSlotUI>();
        if (draggedSlot != null && draggedSlot.presetItem != null)
        {
            // 🔥 PlayerInventory를 찾아서 인벤토리 슬롯 수량 체크
            PlayerInventory inventory = draggedSlot.p.inventory;
            if (inventory != null && inventory.slots[draggedSlot.index].quantity > 0)
            {
                if (draggedSlot != null)
                {
                    int quantity = inventory.slots[draggedSlot.index].quantity;
                    SetItem(draggedSlot.presetItem, draggedSlot.icon?.sprite, quantity);
                }
            }
            else
            {
                Debug.Log("❌ 수량이 부족해서 퀵슬롯에 등록할 수 없습니다.");
            }
        }
    }
}