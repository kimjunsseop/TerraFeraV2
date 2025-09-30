using UnityEngine;
using UnityEngine.Tilemaps;
public class PlaceBuilding : MonoBehaviour
{
    public PlayerInventory inventory;          // 플레이어 인벤토리 참조
    public Tilemap groundTilemap;              // 땅 타일맵
    public Tilemap buildingTilemap;            // 건축물용 타일맵
    public TileBase[] buildingTile;              // 건축에 사용할 타일
    public ItemData itemData;                  // 장착한 아이템 데이터
    public GameObject equippedBuilding;        // 장착한 건축물 프리팹 오브젝트
    public Transform playerTransform;          // 플레이어 Transform
    private GameObject ghostPreview;
    public Material ghostMaterial;

    private void Start()
    {
        //CreateGhostPreview();
    }

    private void Update()
    {
        if (ghostPreview != null)
        {
            UpdateGhostPreviewPosition();
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (!inventory.isInventoryOpen)
            {
                TryPlaceBuilding();
            }
        }

    }

    void TryPlaceBuilding()
    {
        if (itemData == null || equippedBuilding == null) return;

        if (inventory.GetItemCount(itemData) <= 0)
        {
            Debug.Log("❌ 설치할 수 있는 건축물이 없습니다.");
            inventory.UnEquip();
            DestroyGhost();
            return;
        }

        Vector3 placePos = GetPlacementWorldPosition();
        Debug.Log($"[GetPlacementWorldPosition] 계산된 설치 위치 (world): {placePos}");

        // 셀 좌표로 변환 후 땅 타일 유효성 검사
        Vector3Int groundCellPos = groundTilemap.WorldToCell(placePos);
        if (!groundTilemap.HasTile(groundCellPos))
        {
            Debug.Log($"❌ 설치할 땅이 없습니다: {groundCellPos}");
            return;
        }
        // 건축물 있는지 확인
        Vector3 checkPosition = placePos + new Vector3(0f, 0.5f, 0f);
        Vector3 boxSize = new Vector3(0.25f, 1f, 0.25f);
        Collider[] colliders = Physics.OverlapBox(checkPosition, boxSize);
        foreach (var col in colliders)
        {
            if (col.gameObject.layer != LayerMask.NameToLayer("Ground") && col.gameObject.layer != LayerMask.NameToLayer("builing") && col.gameObject.layer != LayerMask.NameToLayer("Default"))
            {
                Debug.Log("설치불가");
                return;
            }
        }
        // 없으면 설치 
        Quaternion rotation = GetPlacementRotation();

        GameObject building = Instantiate(equippedBuilding, placePos, rotation, buildingTilemap.transform);
        Door door = building.GetComponentInChildren<Door>();
        if (door != null)
        {
            Vector3 installDirection = playerTransform.forward;
            installDirection.y = 0f;
            door.SetInstallDirection(installDirection);
        }

        Collider buildingCollider = building.GetComponentInChildren<Collider>();
        if (buildingCollider != null)
        {
            buildingCollider.enabled = true;
        }

        Rigidbody buildingRb = building.GetComponentInChildren<Rigidbody>();
        if (buildingRb != null)
        {
            buildingRb.isKinematic = true;
            buildingRb.useGravity = false;
        }
        AudioManager.Instance.PlaySFX(inventory.player.buildingSound);
        Debug.Log($"✅ 건축물 설치 성공 (worldPos): {placePos}");

        inventory.RemoveItem(itemData, 1);
        inventory.UpdateUI();

        if (inventory.GetItemCount(itemData) <= 0)
        {
            inventory.UnEquip();
            DestroyGhost();
            Debug.Log("🛑 설치할 건축물이 모두 소진되어 장비 해제됨.");
        }
    }

    Vector3 GetPlacementWorldPosition()
    {
        Vector3 playerWorldPos = playerTransform.position;

        // 1. 플레이어 위치를 0.5 단위로 스냅
        float snappedX = Mathf.Round(playerWorldPos.x * 2f) / 2f;
        float snappedZ = Mathf.Round(playerWorldPos.z * 2f) / 2f;

        // 2. 바라보는 방향 읽기
        Vector3 forward = playerTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        float targetX = snappedX;
        float targetZ = snappedZ;

        // 3. 절대값 비교로 이동할 축 결정
        if (Mathf.Abs(forward.x) > Mathf.Abs(forward.z))
        {
            if (forward.x > 0)
                targetX += 1f;
            else
                targetX -= 1f;
        }
        else
        {
            if (forward.z > 0)
                targetZ += 1f;
            else
                targetZ -= 1f;
        }

        // 4. 이동한 위치도 0.5 단위로 스냅
        targetX = Mathf.Round(targetX * 2f) / 2f;
        targetZ = Mathf.Round(targetZ * 2f) / 2f;

        // 5. 월드 좌표 반환 (y는 그대로 유지)
        return new Vector3(targetX, 0f, targetZ);
    }

    Quaternion GetPlacementRotation()
    {
        Vector3 forward = playerTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        if (Mathf.Abs(forward.x) > Mathf.Abs(forward.z))
        {
            return Quaternion.Euler(0, 90, 0); // X축 가로 방향 설치
        }
        else
        {
            return Quaternion.Euler(0, 0, 0); // Z축 세로 방향 설치
        }
    }

    void CreateGhostPreview()
    {
        if (equippedBuilding != null)
        {
            ghostPreview = Instantiate(equippedBuilding);
            DestroyImmediate(ghostPreview.GetComponent<Collider>());
            DestroyImmediate(ghostPreview.GetComponent<Rigidbody>());
            Renderer[] renderers = ghostPreview.GetComponentsInChildren<Renderer>();
            foreach (var render in renderers)
            {
                render.material = ghostMaterial;
            }
        }
    }

    void UpdateGhostPreviewPosition()
    {
        Vector3 ghostPos = GetPlacementWorldPosition();
        Quaternion ghostRot = GetPlacementRotation();
        ghostPreview.transform.position = ghostPos;
        ghostPreview.transform.rotation = ghostRot;
    }

    public void SetItemData(ItemData newItemData, GameObject newEquipped)
    {
        DestroyGhost();

        itemData = newItemData;
        equippedBuilding = newEquipped;
        CreateGhostPreview();
    }

    public void DestroyGhost()
    {
        if (ghostPreview != null)
        {
            Destroy(ghostPreview);
        }
    }
}