using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class AggressiveAnimalDen : MonoBehaviour
{
    [Header("Animal Settings")]
    public string animalName = "동물";
    public GameObject animalPrefab;
    public float spawnRadius = 20f;
    public float innerRadius = 5f; // 🔸추가: 내부에서 스폰 방지할 최소 반경
    public int maxAnimals = 5;
    public float animalMoveRange = 30f;
    public float sampleDistance = 8f;

    private List<GameObject> spawnedAnimals = new();

    public void TrySpawnAnimal()
    {
        spawnedAnimals.RemoveAll(a => a == null);

        if (spawnedAnimals.Count >= maxAnimals) return;
        if (!AggressiveAnimalManager.Instance.CanSpawn()) return;

        for (int i = 0; i < 10; i++)
        {
            Vector2 offset = RandomPointInAnnulus(innerRadius, spawnRadius); // 🔸변경
            Vector3 spawnPos = transform.position + new Vector3(offset.x, 0f, offset.y);

            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
            {
                Vector3 fixedPos = hit.position;
                fixedPos.y += 0.3f;

                GameObject animal = Instantiate(animalPrefab, fixedPos, Quaternion.identity);

                AggressiveAnimalAI ai = animal.GetComponent<AggressiveAnimalAI>();
                if (ai != null)
                {
                    ai.SetDenCenter(transform);
                    ai.normalMoveRange = animalMoveRange;
                }

                spawnedAnimals.Add(animal);
                AggressiveAnimalManager.Instance.RegisterAnimal();

                Debug.Log($"[{name}] {animalName} 생성됨 (현재 {animalName} {spawnedAnimals.Count}마리, 전체 {AggressiveAnimalManager.Instance.GetCurrentCount()}/{AggressiveAnimalManager.Instance.GetMaxCount()}마리)");

                return;
            }
        }

        Debug.LogWarning($"{name}: {animalName} 생성 실패 (NavMesh 못 찾음)");
    }

    // 🔸 도넛 형태의 위치 반환 함수
    private Vector2 RandomPointInAnnulus(float minRadius, float maxRadius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Mathf.Sqrt(Random.Range(minRadius * minRadius, maxRadius * maxRadius));
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius); // 바깥 원
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, innerRadius); // 🔸내부 원 (스폰 금지 영역)
    }
}
