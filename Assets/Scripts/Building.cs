using UnityEngine;

public class Building : MonoBehaviour
{
    [Header("Building Health Settings")]
    public float maxHealth = 30f;
    public float currentHealth;
    //public GameObject destructionEffect;
    public string prefabName;
    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} 건축물 피해! 현재 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }

    void DestroyBuilding()
    {
        // if (destructionEffect != null)
        // {
        //     Instantiate(destructionEffect, transform.position, Quaternion.identity);
        // }

        Destroy(gameObject);
    }
}
