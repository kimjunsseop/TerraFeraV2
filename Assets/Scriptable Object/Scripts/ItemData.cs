using System;
using UnityEngine;

public enum ItemType
{
    Resource,
    Equipable,
    Consumable,
    Building,
    Wearable
}

public enum ConsumableType
{
    Hunger,
    Health
}

[Serializable]
public class ItemDataConsumable
{
    public ConsumableType type;
    public float value;
}

[CreateAssetMenu(fileName = "Item", menuName = "New Item")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string displayName;
    public string description;
    public ItemType type;
    public Sprite icon;
    public GameObject dropPrefab;

    [Header("Stacking")]
    public bool canStack;
    public int maxStackAmount;

    [Header("Consumable")]
    public ItemDataConsumable[] consumables;
    public float full;
    public float healing;

    [Header("Crafting")]
    public bool isCraftable;
    public ItemData[] requiredItems;
    public int[] requiredAmount;

    [Header("Building")]
    public GameObject prefabToPlace;  // 설치용 프리팹

    [Header("Equipable State")]
    public int attackPower;
    public int woodAttackPower;
    public int stoneAttackPower;
    [Header("Wearable State")]
    public float heat;
}