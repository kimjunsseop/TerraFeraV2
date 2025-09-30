using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveManager
{
    public static void SaveGame()
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = "save_" + timestamp + ".json";
        string savePath = Path.Combine(Application.persistentDataPath, fileName);
        SaveData data = new SaveData();
        player p = GameObject.FindWithTag("Player").GetComponent<player>();
        PlayerInventory inventory = p.GetComponent<PlayerInventory>();

        data.player = new PlayerData
        {
            position = p.transform.position,
            health = p.health.curValue,
            hunger = p.hunger.curValue,
            temperature = p.temperature.curValue,
            inventory = ConvertToItemSlotData(inventory.slots),
            //quickSlots = ConvertToQuickSlotData(inventory.quickSlots)
        };

        data.trees = new List<ResourceData>();
        foreach (var tree in GameObject.FindObjectsOfType<Tree>())
        {
            data.trees.Add(new ResourceData
            {
                type = "Tree",
                prefabName = tree.gameObject.name.Replace("(Clone)","").Trim(),
                position = tree.transform.position,
                health = tree.health
            });
        }

        data.stones = new List<ResourceData>();
        foreach (var stone in GameObject.FindObjectsOfType<Stone>())
        {
            data.stones.Add(new ResourceData
            {
                type = "Stone",
                prefabName = stone.gameObject.name.Replace("(Clone)","").Trim(),
                position = stone.transform.position,
                health = stone.health
            });
        }

        data.buildings = new List<BuildingData>();
        foreach (var building in GameObject.FindObjectsOfType<Building>())
        {
            Vector3 installDir = Vector3.forward;
            Door door = building.GetComponentInChildren<Door>();
            if (door != null)
            {
                installDir = door.GetInstallForward();
            }
            data.buildings.Add(new BuildingData
            {
                prefabName = building.prefabName,
                position = building.transform.position,
                rotation = building.transform.rotation,
                health = building.currentHealth,
                installDirection = installDir
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("저장완료: " + savePath);
    }

    public static void LoadGame(string savePath)
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("저장 파일 없음: " + savePath);
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        foreach (var obj in Object.FindObjectsOfType<Tree>()) Object.Destroy(obj.gameObject);
        foreach (var obj in Object.FindObjectsOfType<Stone>()) Object.Destroy(obj.gameObject);
        foreach (var obj in Object.FindObjectsOfType<Building>()) Object.Destroy(obj.gameObject);
        Debug.Log("삭제완료");
        player p = GameObject.FindWithTag("Player").GetComponent<player>();
        PlayerInventory inventory = p.GetComponent<PlayerInventory>();

        p.transform.position = data.player.position;
        p.health.curValue = data.player.health;
        p.hunger.curValue = data.player.hunger;
        p.temperature.curValue = data.player.temperature;

        //inventory.ClearInventory();

        foreach (var slotData in data.player.inventory)
        {
            ItemData item = Resources.Load<ItemData>("Items/" + slotData.itemName);
            if (item == null) continue;

            for (int i = 0; i < inventory.slots.Count; i++)
            {
                if (inventory.slots[i].item == null || inventory.slots[i].item.name == item.name)
                {
                    inventory.slots[i].item = item;
                    inventory.slots[i].item.icon = item.icon;
                    inventory.slots[i].quantity = slotData.quantity;
                    break;
                }
            }
        }
        inventory.UpdateUI();

        foreach (var t in data.trees)
        {
            GameObject tree = GameObject.Instantiate(Resources.Load<GameObject>("Resources/" + t.prefabName), t.position, Quaternion.identity);
            tree.GetComponent<Tree>().health = t.health;
        }

        foreach (var s in data.stones)
        {
            GameObject stone = GameObject.Instantiate(Resources.Load<GameObject>("Resources/" + s.prefabName), s.position, Quaternion.identity);
            stone.GetComponent<Stone>().health = s.health;
        }

        foreach (var b in data.buildings)
        {
            GameObject prefab = Resources.Load<GameObject>("Buildings/" + b.prefabName);
            if (prefab == null)
            {
                Debug.LogError("❌ 프리팹 로드 실패: " + b.prefabName);
                continue;
            }
            GameObject go = GameObject.Instantiate(prefab, b.position, b.rotation);

            
            Rigidbody rb = go.GetComponentInChildren<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            
            Collider col = go.GetComponentInChildren<Collider>();
            if (col != null)
            {   
                col.enabled = true;
            }
            Door door = go.GetComponentInChildren<Door>();
            if (door != null)
            {
                door.SetInstallDirection(b.installDirection);
            }

            
            go.transform.SetParent(GameObject.Find("BuildingTilemap")?.transform); 

            go.GetComponent<Building>().currentHealth = b.health;
        }
        Debug.Log("저장 위치: " + data.player.position);
        Debug.Log("적용 후 위치: " + p.transform.position);
        Debug.Log("불러오기 완료");
    }

    private static List<ItemSlotData> ConvertToItemSlotData(List<ItemSlot> slots)
    {
        var result = new List<ItemSlotData>();
        foreach (var slot in slots)
        {
            if (slot.item != null && slot.quantity > 0)
            {
                result.Add(new ItemSlotData
                {
                    itemName = slot.item.name,
                    quantity = slot.quantity
                });
            }
        }
        return result;
    }

    private static List<QuickSlotData> ConvertToQuickSlotData(QuickSlotUI[] quickSlots)
    {
        var result = new List<QuickSlotData>();
        for (int i = 0; i < quickSlots.Length; i++)
        {
            var q = quickSlots[i];
            if (q.itemData != null)
            {
                result.Add(new QuickSlotData
                {
                    slotIndex = i,
                    itemName = q.itemData.name,
                    quantity = q.quantity
                });
            }
        }
        return result;
    }
}
