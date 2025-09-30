using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;

[System.Serializable]
public class ItemSlot
{
    public ItemData item;
    public int quantity;
}
[System.Serializable]
public class QuickSlot
{
    public ItemData item;
}

public class PlayerInventory : MonoBehaviour
{
    public GameObject inventoryUI; // UI Canavs 
    public Transform dropPosition; // 아이템 던질곳
    public ItemSlotUI[] uiSlots; // 인벤토리 슬롯 올라갈 공간
    public QuickSlotUI[] quickSlots;
    public List<ItemSlot> slots = new List<ItemSlot>(); // 실제 슬롯

    public bool isInventoryOpen = false; // 오픈 여부
    public Transform hand; // 손 위치
    public Transform back;
    public GameObject equipped; // 들고있는거
    public GameObject weared;
    // 버튼들들
    public Button useButton;
    public Button equipButton;
    public Button unEquipButton;
    public Button dropButton;
    public Button makeButton;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public Tilemap groundTilemap;
    public Tilemap buildingTilemap;
    public TileBase[] buildingTile;
    public Material ghostMaterial;
    public player player;

    private int selectedSlotIndex = -1;

    private void Start()
    {
        if (PlayerPrefs.GetInt("ClearInventory", 0) == 1)
        {
            ClearInventory();
            PlayerPrefs.SetInt("ClearInventory", 0);
        }
        // 인벤토리 비활성화 (초기값)
        inventoryUI.SetActive(false);
        // 슬롯 ui 추가한 숫자만큼 거기다가 아이콘 미리 넣어놓고 갯수 0으로 셋팅팅
        for (int i = 0; i < uiSlots.Length; i++)
        {
            slots.Add(new ItemSlot { item = uiSlots[i].presetItem, quantity = 0 });
            uiSlots[i].index = i;
            uiSlots[i].Set(uiSlots[i].presetItem.icon, 0);
        }
        //버튼 메서드 연결
        useButton.onClick.AddListener(OnUseButtonClicked);
        equipButton.onClick.AddListener(OnEquipButtonClicked);
        unEquipButton.onClick.AddListener(OnUnEquipButtonClicked);
        dropButton.onClick.AddListener(OnDropButtonClicked);
        makeButton.onClick.AddListener(OnMakeButtonClicked);
        // 초기 버튼 비활성화
        useButton.gameObject.SetActive(false);
        equipButton.gameObject.SetActive(false);
        unEquipButton.gameObject.SetActive(false);
        dropButton.gameObject.SetActive(false);
        makeButton.gameObject.SetActive(false);
        itemNameText.text = "";
        itemDescriptionText.text = "";
        player = GetComponent<player>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) ToggleInventory();
        for (int i = 0; i < quickSlots.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {

                EquipQuickSlot(i);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isInventoryOpen)
            {
                isInventoryOpen = !isInventoryOpen;
                inventoryUI.SetActive(isInventoryOpen);
            }
        }
    }

    void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryUI.SetActive(isInventoryOpen);
        Cursor.lockState = isInventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isInventoryOpen;
        UpdateUI();
    }

    public void AddItem(ItemData item)
    {
        foreach (var slot in slots)
        {
            if (slot.item == item)
            {
                slot.quantity++;
                UpdateUI();
                return;
            }
        }
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].quantity == 0)
            {
                slots[i].item = item;
                slots[i].quantity = 1;
                UpdateUI();
                return;
            }
        }
    }

    public void UseItem(int index)
    {
        if (slots[index].quantity <= 0)
        {
            Debug.Log("인벤토리에 아이템 없음");
            return;
        }
        slots[index].quantity--;
        ItemData item = slots[index].item;
        if (item.type == ItemType.Consumable)
        {
            if (item.displayName == "Meat")
            {
                if (player.hunger.curValue == player.hunger.maxValue)
                {
                    return;
                }
                float hungerToRecover = Mathf.Min(item.full, player.hunger.maxValue - player.hunger.curValue);
                player.hunger.curValue += hungerToRecover;
            }
            if (item.displayName == "Pear")
            {
                if (player.health.curValue == player.health.maxValue)
                {
                    return;
                }
                float healToRecover = Mathf.Min(item.healing, player.health.maxValue - player.health.curValue);
                player.health.curValue += healToRecover;
            }
            if (item.displayName == "Apple")
            {
                if(player.hunger.curValue == player.hunger.maxValue)
                {
                    return;
                }
                float hungerToRecover = Mathf.Min(item.full, player.hunger.maxValue - player.hunger.curValue);
                player.hunger.curValue += hungerToRecover;
            }
        }
        for (int i = 0; i < quickSlots.Length; i++)
        {
            if (quickSlots[i] != null && quickSlots[i].itemData == item)
            {
                quickSlots[i].quantity = slots[index].quantity;
                quickSlots[i].quantityText.text = quickSlots[i].quantity.ToString();
            }
        }
        UpdateUI();
    }

    public void DropItem(int index)
    {
        if (slots[index].quantity > 0)
        {
            Vector3 dropOffset = dropPosition.forward * 1.5f + dropPosition.position + Vector3.up * 1.0f;
            Vector3 dropPos = dropPosition.position + dropOffset;
            GameObject droppedItem = Instantiate(slots[index].item.dropPrefab, dropOffset, Quaternion.identity);

            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            Collider col = droppedItem.GetComponent<Collider>();
            if (rb != null) rb.isKinematic = false;
            if (col != null) col.enabled = true;

            slots[index].quantity--;

            // 손에 들고 있는 아이템을 버리면 해제
            if (equipped != null && equipped.name.Contains(slots[index].item.name))
            {
                Destroy(equipped);
                equipped = null;
            }

            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        for (int i = 0; i < uiSlots.Length; i++)
        {
            uiSlots[i].Set(slots[i].item.icon, slots[i].quantity);
        }
    }

    public void OnSlotClicked(int index)
    {
        selectedSlotIndex = index;
        ItemSlot selectedSlot = slots[selectedSlotIndex];
        ItemData selectedItem = selectedSlot.item;
        bool isEquippedItem = false;
        if (selectedItem.type == ItemType.Wearable)
        {
            isEquippedItem = weared != null;
        }
        else
        {
            isEquippedItem = equipped != null && equipped.name.Contains(selectedItem.name);
        }
        itemNameText.text = selectedItem.displayName;
        itemDescriptionText.text = selectedItem.description;
        useButton.gameObject.SetActive(selectedItem.type == ItemType.Consumable);
        equipButton.gameObject.SetActive(selectedItem.type == ItemType.Equipable || selectedItem.type == ItemType.Building || selectedItem.type == ItemType.Wearable);
        unEquipButton.gameObject.SetActive(isEquippedItem);
        dropButton.gameObject.SetActive(true);
        makeButton.gameObject.SetActive(selectedItem.isCraftable && CanCraft(selectedItem));
    }

    public void OnUseButtonClicked() { if (selectedSlotIndex >= 0) UseItem(selectedSlotIndex); }
    public void OnEquipButtonClicked() { if (selectedSlotIndex >= 0) Equip(slots[selectedSlotIndex].item); }
    public void OnUnEquipButtonClicked() { if (selectedSlotIndex >= 0) UnEquip(); }
    public void OnDropButtonClicked() { if (selectedSlotIndex >= 0) DropItem(selectedSlotIndex); }
    public void OnMakeButtonClicked()
    {
        // 이미 있으면 추가 불가.
        if (selectedSlotIndex < 0)
        {
            return;
        }
        AudioManager.Instance.PlaySFX(player.makeSound);
        ItemSlot selectedSlot = slots[selectedSlotIndex];
        ItemData selectedItem = selectedSlot.item;
        if (CanCraft(selectedItem))
        {
            for (int i = 0; i < selectedItem.requiredItems.Length; i++)
            {
                RemoveItem(selectedItem.requiredItems[i], selectedItem.requiredAmount[i]);
            }
            selectedSlot.quantity++;
            UpdateUI();
        }
    }
    public bool CanCraft(ItemData item)
    {
        for (int i = 0; i < item.requiredItems.Length; i++)
        {
            if (GetItemCount(item.requiredItems[i]) < item.requiredAmount[i])
            {
                return false;
            }
        }
        return true;
    }
    public int GetItemCount(ItemData item)
    {
        foreach (var slot in slots)
        {
            if (slot.item == item)
            {
                return slot.quantity;
            }
        }
        return 0;
    }

    public void RemoveItem(ItemData item, int amount)
    {
        foreach (var slot in slots)
        {
            if (slot.item == item)
            {
                slot.quantity -= amount;
                return;
            }
        }
    }
    public void Equip(ItemData item)
    {
        int itemCount = GetItemCount(item);
        if (itemCount <= 0)
        {
            Debug.Log("이 아이템이 인벤토리에 없습니다.");
            return;
        }
        AudioManager.Instance.PlaySFX(player.equipSound);
        if (item.type == ItemType.Equipable || item.type == ItemType.Building)
        {
            PlaceBuilding prePlacer = hand.GetComponent<PlaceBuilding>();
            if (prePlacer != null)
            {
                prePlacer.DestroyGhost();
                Destroy(prePlacer);
            }
            if (equipped != null) Destroy(equipped);
            equipped = Instantiate(item.dropPrefab, hand);
            equipped.transform.localPosition = Vector3.zero;
            equipped.transform.localRotation = Quaternion.identity;
            Collider collider = equipped.GetComponentInChildren<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Rigidbody rb = equipped.GetComponentInChildren<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;  // 물리 연산 중지
                rb.useGravity = false;  // 중력 제거
            }
            if (item.type == ItemType.Equipable)
            {
                Debug.Log($"Equipable 장착됨 : {item.displayName}");
                if (item.displayName == "Knife")
                {
                    player.attackDamage = player.basicAttackDamage + item.attackPower;
                }
                if (item.displayName == "Axe")
                {
                    player.woodAttack = player.basicAttackDamage + item.woodAttackPower;
                }
                if (item.displayName == "pickax")
                {
                    player.stoneAttack = player.basicAttackDamage + item.stoneAttackPower;
                }
            }
            else if (item.type == ItemType.Building)
            {
                Debug.Log($"건축 아이템 장착됨 : {item.displayName}");

                // 설치 가능한 건물로 마킹
                PlaceBuilding oldPlacer = hand.gameObject.GetComponent<PlaceBuilding>();
                if (oldPlacer != null)
                {
                    oldPlacer.DestroyGhost();
                    Destroy(oldPlacer);
                }
                // PlacerBuilding 스크립트 변수 값 대입하는 부분
                PlaceBuilding placer = hand.gameObject.AddComponent<PlaceBuilding>();
                placer.groundTilemap = groundTilemap;
                placer.buildingTilemap = buildingTilemap;
                placer.inventory = this;
                placer.itemData = item;
                placer.equippedBuilding = equipped;
                placer.buildingTile = buildingTile;
                placer.playerTransform = transform;
                placer.ghostMaterial = ghostMaterial;
                placer.SetItemData(item, equipped);
            }
        }
        else if (item.type == ItemType.Wearable)
        {
            if (weared != null) Destroy(weared);
            weared = Instantiate(item.dropPrefab, back);
            weared.transform.localPosition = Vector3.zero;
            weared.transform.localRotation = Quaternion.identity;
            Debug.Log($"Wearable 장착됨 : {item.displayName}");
            if (item.displayName == "Cloth")
            {
                player.temperature.decayRate -= item.heat;
            }
        }
    }

    public void UnEquip()
    {
        PlaceBuilding pplacer = hand.GetComponent<PlaceBuilding>();
        if (pplacer != null)
        {
            pplacer.DestroyGhost();
            Destroy(pplacer);
        }
        if (equipped != null)
        {
            player.attackDamage = player.basicAttackDamage;
            player.stoneAttack = player.basicAttackDamage;
            player.woodAttack = player.basicAttackDamage;
            Destroy(equipped);
        }
        if (weared != null)
        {
            // default decayRate로 다시 초기화.
            player.temperature.decayRate = 1.0f;
            Destroy(weared);
        }
        equipped = null;
        weared = null;
        unEquipButton.gameObject.SetActive(false);
        equipButton.gameObject.SetActive(true);
    }
    public void EquipQuickSlot(int index)
    {
        if (quickSlots[index] == null || quickSlots[index].itemData == null) return;

        ItemData quickItem = quickSlots[index].itemData;

        int invIndex = GetItemSlotIndex(quickItem);
        if (invIndex == -1)
        {
            return;
        }
        bool isAlreadyEquipped = false;

        if (quickItem.type == ItemType.Equipable || quickItem.type == ItemType.Building)
        {
            if (equipped != null)
            {
                isAlreadyEquipped = true;
            }
        }
        else if (quickItem.type == ItemType.Wearable)
        {
            if (weared != null)
            {
                isAlreadyEquipped = true;
            }
        }

        if (isAlreadyEquipped)
        {
            UnEquip();
            return;
        }
        if (quickItem.type == ItemType.Consumable)
        {
            UseItem(invIndex);
            return;
        }
        UnEquip();
        Equip(quickItem);
    }
    public int GetItemSlotIndex(ItemData item)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
                return i;
        }
        return -1;
    }
    public bool IsHoldingBuilding()
    {
        return equipped != null && equipped.GetComponent<PlaceBuilding>() != null;
    }
    public void ClearInventory()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].quantity = 0;
        }
        foreach (var q in quickSlots)
        {
            if (q == null)
            {
                continue;
            }
            q.itemData = null;
            q.icon.sprite = null;
            q.quantity = 0;
            if (q.quantityText != null)
            {
                q.quantityText.text = "";
            }
        }
        UpdateUI();
    }
}