// PassiveAnimalDen.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PassiveAnimalDen : MonoBehaviour
{
    public string animalName = "�¼��� ����";
    public GameObject passiveAnimalPrefab;
    public int maxSpawnCount = 5;
    public float spawnRadius = 30f;
    public float respawnInterval = 30f;

    private List<GameObject> spawnedAnimals = new();

    void Start()
    {
        for (int i = 0; i < maxSpawnCount; i++)
        {
            TrySpawn();
        }
        InvokeRepeating(nameof(RespawnCheck), respawnInterval, respawnInterval);
    }

    public void TrySpawn()
    {
        spawnedAnimals.RemoveAll(a => a == null);
        if (spawnedAnimals.Count >= maxSpawnCount) return;

        for (int i = 0; i < 10; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(offset.x, 0, offset.y);

            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            {
                GameObject animal = Instantiate(passiveAnimalPrefab, hit.position, Quaternion.identity);
                PassiveAnimalAI ai = animal.GetComponent<PassiveAnimalAI>();
                if (ai != null) ai.SetDen(this);
                spawnedAnimals.Add(animal);

                Debug.Log($"[{name}] {animalName} ������ (���� {animalName} {spawnedAnimals.Count}���� / �ִ� {maxSpawnCount}����)");
                return;
            }
        }

        Debug.LogWarning($"{name}: {animalName} ���� ���� (NavMesh �� ã��)");
    }

    public void UnregisterAnimal()
    {
        spawnedAnimals.RemoveAll(a => a == null);
    }

    void RespawnCheck()
    {
        TrySpawn();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
