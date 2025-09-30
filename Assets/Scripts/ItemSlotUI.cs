using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image icon;
    public TextMeshProUGUI quantityText;
    public int index;
    public Button button;
    public player p;
    public ItemData presetItem; // 각 슬롯이 기본적으로 어떤 아이템을 담을지 미리 설정
    private Transform originalParent;

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning("Button not assigned in ItemSlotUI!");
        }

        if (presetItem != null)
        {
            icon.sprite = presetItem.icon; // 기본 아이콘 설정
            icon.color = new Color(1, 1, 1, 0.3f); // 기본적으로 흐리게 표시 (투명도 낮춤)
            quantityText.text = "0";
        }
        else
        {
            icon.enabled = false; // presetItem이 없으면 아이콘 숨김
            quantityText.enabled = false;
        }
    }

    public void Set(Sprite newIcon, int newQuantity)
    {
        icon.sprite = newIcon;
        quantityText.text = newQuantity.ToString();
        icon.color = new Color(1, 1, 1, newQuantity > 0 ? 1.0f : 0.3f); // 개수가 있으면 선명하게, 없으면 흐리게
        icon.enabled = true;
        quantityText.enabled = true;
    }

    public void Clear()
    {
        if (presetItem != null)
        {
            icon.sprite = presetItem.icon;
            quantityText.text = "0";
            icon.color = new Color(1, 1, 1, 0.3f);
        }
        else
        {
            icon.enabled = false;
            quantityText.enabled = false;
        }
    }

    public void OnButtonClicked()
    {
        if (p != null && p.inventory != null)
        {
            p.inventory.OnSlotClicked(index);
        }
        else
        {
            Debug.LogWarning($"[ItemSlotUI] 플레이어 또는 인벤토리 참조가 없음!");
        }
    }

   public void OnBeginDrag(PointerEventData eventData)
    {
        if (presetItem == null || icon == null) return;
        originalParent = transform.parent;
        Canvas canvas = GetComponentInParent<Canvas>();
        if(canvas != null)
        {
            transform.SetParent(canvas.transform);
            transform.SetAsLastSibling();
        }
        icon.raycastTarget = false;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        var quickSlot = eventData.pointerEnter?.GetComponent<QuickSlotUI>();
        if (quickSlot != null)
        {
            // QuickSlot에 드랍 성공
            quickSlot.SetItem(presetItem, presetItem.icon);
        }
    
        transform.SetParent(originalParent, false); // 기존의 grid layout 으로 복귀
        icon.raycastTarget = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (presetItem == null) return;
        transform.position = eventData.position;
    }

}