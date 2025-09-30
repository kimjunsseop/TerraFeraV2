using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public PlayerData player;
    public List<ResourceData> trees;
    public List<ResourceData> stones;
    public List<BuildingData> buildings;
    public List<AnimalData> animals;
}
[System.Serializable]
public class PlayerData
{
    public Vector3 position;
    public float health, hunger, temperature;
    public List<ItemSlotData> inventory;
    //public List<QuickSlotData> quickSlots;
}
[System.Serializable]
public class ItemSlotData
{
    public string itemName;
    public int quantity;
}
[System.Serializable]
public class QuickSlotData
{
    public int slotIndex;
    public string itemName;
    public int quantity;
}
[System.Serializable]
public class ResourceData
{
    public string type;
    public string prefabName;
    public Vector3 position;
    public int health;
}
[System.Serializable]
public class BuildingData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    public float health;
    public Vector3 installDirection;
}
[System.Serializable]
public class AnimalData
{
    public string prefabName;
    public Vector3 position;
    public int health;
}