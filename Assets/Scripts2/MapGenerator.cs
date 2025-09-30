using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("맵 설정")]
    public int mapWidth = 500;
    public int mapHeight = 500;
    public Tilemap tilemap;
    public Tile waterTile;
    public Tile snowTile;
    public Tile grassTile;
    public Tile desertTile;

    //[Header("자원 프리팹")]
    //public GameObject treePrefab;
    //public GameObject rockPrefab;

    //[Header("자원 숲 클러스터 설정")]
    //public int forestClusterCount = 30;
    //public float forestClusterRadius = 10f;

    public void GenerateMap()
    {
#if UNITY_EDITOR
        foreach (Transform child in transform)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }
        tilemap.ClearAllTiles();
#endif

        Vector2 center = new Vector2(mapWidth / 2f, mapHeight / 2f);

        //List<Vector2Int> forestCenters = GenerateForestCenters(forestClusterCount);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                bool isLand = IsLand(x, y, center);

                Tile chosenTile = isLand ? GetTileByRegion(y) : waterTile;
                tilemap.SetTile(tilePos, chosenTile);

                // 자원 스폰 제거
                /*
                if (isLand)
                {
                    Vector3 world = tilemap.CellToWorld(tilePos);
                    Vector3 spawnXZ = new Vector3(world.x, 0f, world.z);

                    bool isInForest = IsNearForest(x, y, forestCenters);
                    if (isInForest && Random.value < 0.25f)
                    {
                        SpawnResource(treePrefab, rockPrefab, spawnXZ);
                    }
                    else if (Random.value < 0.01f)
                    {
                        SpawnResource(treePrefab, rockPrefab, spawnXZ);
                    }
                }
                */
            }
        }
    }

    bool IsLand(int x, int y, Vector2 center)
    {
        float distance = Vector2.Distance(new Vector2(x, y), center);
        float noise = Mathf.PerlinNoise(x * 0.01f, y * 0.01f);
        float adjustedDistance = distance - noise * 40f;
        return adjustedDistance < 220f;
    }

    Tile GetTileByRegion(int y)
    {
        if (y > mapHeight * 0.75f) return snowTile;
        else if (y > mapHeight * 0.25f) return grassTile;
        else return desertTile;
    }

    /*
    void SpawnResource(GameObject tree, GameObject rock, Vector3 spawnXZ)
    {
        GameObject prefab = Random.value < 0.5f ? tree : rock;
        Vector3 finalPosition = spawnXZ + Vector3.up * 0.5f;
        Instantiate(prefab, finalPosition, Quaternion.identity, transform);
    }

    List<Vector2Int> GenerateForestCenters(int count)
    {
        List<Vector2Int> centers = new List<Vector2Int>();
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(50, mapWidth - 50);
            int y = Random.Range(50, mapHeight - 50);
            centers.Add(new Vector2Int(x, y));
        }
        return centers;
    }

    bool IsNearForest(int x, int y, List<Vector2Int> centers)
    {
        foreach (var center in centers)
        {
            if (Vector2Int.Distance(new Vector2Int(x, y), center) <= forestClusterRadius)
                return true;
        }
        return false;
    }
    */
}
