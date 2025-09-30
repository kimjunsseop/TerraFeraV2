using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public int health = 5; // 플레이어 공격을 5번 맞으면 쓰러짐
    public GameObject dropItemPrefab; // 드랍할 공 (아이템) 프리팹
    Vector3 dropPoint; // 아이템이 생성될 위치
    public float dropForce = 10f; // 공이 튕겨나가는 힘
    public string prefabName;
    void Start()
    {
        dropPoint = transform.position;
        // 수정 부분
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("tree hit! 남은체력: " + health);
        if (health <= 0)
            {
                DropItem();

                // ✅ ResourceSpawner에 파괴 알림
                ResourceSpawner spawner = FindObjectOfType<ResourceSpawner>();
                if (spawner != null)
                {
                    spawner.NotifyResourceDestroyed("Tree", transform.position);
                }

                Destroy(gameObject);
            }
    }

    void DropItem()
    {
        if (dropItemPrefab != null)
        {
            // 아이템을 생성
            GameObject droppedItem = Instantiate(dropItemPrefab, dropPoint, Quaternion.identity);

            // Rigidbody가 있으면 튕겨나가게 만듦
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 트리에서 약간 위쪽으로 튕겨나가게 함
                Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized;
                rb.AddForce(randomDirection * dropForce, ForceMode.Impulse);
            }
        }
    }
}
