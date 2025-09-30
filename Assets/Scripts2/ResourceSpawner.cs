using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourceSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ResourceType
    {
        public string name;                   // 태그 이름: Tree, Stone
        public GameObject[] prefabs;          // 프리팹 배열
        [HideInInspector] public int initialCount;   // 자동 설정
        public float respawnInterval = 10f;   // 리스폰 주기
    }

    [Header("리소스 종류 및 설정")]
    public List<ResourceType> resourceTypes;

    [Header("타일맵 및 스폰 조건")]
    public Tilemap terrainTilemap;              // 타일맵 참조
    public TileBase[] spawnableTiles;           // 자원 생성 가능한 타일 종류
    public LayerMask placementBlockMask;        // 자원 생성이 막혀야 하는 레이어들
    public float checkRadius = 0.5f;            // 겹침 검사 반경

    [Header("리스폰 로직")]
    public float respawnWeightNear = 0.7f;      // 기존 자원 근처 스폰 확률
    public float respawnWeightRandom = 0.3f;    // 무작위 위치 스폰 확률
    public float spawnHeight = 0f;              // Y 높이 보정

    [Header("자원 생성 부모 오브젝트")]
    public Transform treeParent;
    public Transform stoneParent;

    private Dictionary<string, List<Vector3>> existingPositions = new();
    private List<Vector3> spawnablePositions = new();

    void Start()
    {
        CacheSpawnableTilePositions();

        foreach (var type in resourceTypes)
        {
            GameObject[] existing = GameObject.FindGameObjectsWithTag(type.name);
            existingPositions[type.name] = new List<Vector3>();

            foreach (var obj in existing)
                existingPositions[type.name].Add(obj.transform.position);

            type.initialCount = existing.Length;
            Debug.Log($"[{type.name}] 초기 자원 개수: {type.initialCount}");

            StartCoroutine(RespawnRoutine(type));
        }
    }

    void CacheSpawnableTilePositions()
    {
        spawnablePositions.Clear();
        BoundsInt bounds = terrainTilemap.cellBounds;

        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = terrainTilemap.GetTile(cell);

                if (tile != null && System.Array.Exists(spawnableTiles, t => t == tile))
                {
                    Vector3 worldPos = terrainTilemap.CellToWorld(cell) + new Vector3(0.5f, spawnHeight, 0.5f);
                    spawnablePositions.Add(worldPos);
                }
            }
        }
    }

    IEnumerator RespawnRoutine(ResourceType type)
    {
        while (true)
        {
            yield return new WaitForSeconds(type.respawnInterval);

            int currentCount = existingPositions[type.name].Count;
            int respawnNeeded = Mathf.Max(0, type.initialCount - currentCount);

            for (int i = 0; i < respawnNeeded; i++)
            {
                Vector3? spawnPos = GetSpawnPosition(type.name);

                if (spawnPos.HasValue)
                {
                    GameObject prefab = type.prefabs[Random.Range(0, type.prefabs.Length)];
                    Transform parent = GetParentByType(type.name);
                    GameObject resource = Instantiate(prefab, spawnPos.Value, Quaternion.identity, parent);
                    existingPositions[type.name].Add(spawnPos.Value);
                }
            }
        }
    }

    Vector3? GetSpawnPosition(string typeName)
    {
        List<Vector3> existing = existingPositions[typeName];
        Vector3 candidate;

        // 70% 확률: 기존 자원 근처
        if (existing.Count > 0 && Random.value < respawnWeightNear)
        {
            Vector3 center = existing[Random.Range(0, existing.Count)];

            for (int attempt = 0; attempt < 10; attempt++)
            {
                float offsetX = Random.Range(-2.5f, 2.5f);
                float offsetZ = Random.Range(-2.5f, 2.5f);
                Vector3 offset = new Vector3(offsetX, 0f, offsetZ);
                candidate = center + offset;
                candidate.y = spawnHeight;

                if (Vector3.Distance(candidate, center) >= 1.5f && IsValidPosition(candidate))
                    return candidate;
            }
            return null;
        }
        else // 30% 확률: 무작위 후보지
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                candidate = spawnablePositions[Random.Range(0, spawnablePositions.Count)];
                if (IsValidPosition(candidate))
                    return candidate;
            }
            return null;
        }
    }

    bool IsValidPosition(Vector3 pos)
    {
        return !Physics.CheckSphere(pos, checkRadius, placementBlockMask);
    }

    public void NotifyResourceDestroyed(string typeName, Vector3 pos)
    {
        if (existingPositions.ContainsKey(typeName))
            existingPositions[typeName].Remove(pos);
    }

    Transform GetParentByType(string typeName)
    {
        return typeName == "Tree" ? treeParent :
               typeName == "Stone" ? stoneParent : null;
    }
}