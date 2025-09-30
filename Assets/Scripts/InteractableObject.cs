using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string interactMessage = "아이템 줍기"; // UI에 표시될 메시지
    //public string useInteractMessage = "열기";
    public GameObject itemPrefab; // 플레이어가 줍게 될 아이템 프리팹
    public Transform dropPositon;


    public void Interact(PlayerInventory inventory)
    {
        Debug.Log(interactMessage); // 콘솔에 상호작용 메시지 출력

        // 아이템을 줍는 로직 (여기서는 간단히 아이템 프리팹 생성 후 삭제)
        if (itemPrefab == null)
        {
            Instantiate(itemPrefab, transform.position, Quaternion.identity); // 아이템 프리팹 생성
        }
        ItemData itemData = itemPrefab.GetComponent<ItemObjects>().item;
        inventory.AddItem(itemData);
        Destroy(gameObject); // 현재 오브젝트 삭제 (줍는 아이템이라면 아이템 사라짐)
    }
}
