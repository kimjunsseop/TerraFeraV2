using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public float checkRate = 0.05f; // 일정 시간마다 상호작용을 체크하는 간격 (초 단위)
    private float lastCheckTime; // 마지막으로 체크한 시간을 저장하는 변수
    public float maxCheckDistance = 2f; // 상호작용 가능한 최대 거리
    public LayerMask layerMask; // Raycast가 감지할 레이어 (ex: 아이템, 문 등)

    private GameObject curInteractGameobject; // 현재 감지된 상호작용 가능한 오브젝트
    private InteractableObject curInteractable; // 감지된 오브젝트의 InteractableObject 스크립트

    public TextMeshProUGUI promptText; // UI에 표시될 상호작용 안내 텍스트
    private Transform playerTransform; // 플레이어의 Transform (위치, 방향 등)
    public PlayerInventory inventory;
    public GameObject brazierUI;
    public Brazier brazier;
    private bool isBrazierOpen = false;
    // 레이어를 담기위한 변수들
    private int interactableLayer;
    private int brazierLayer;
    private int buildingLayer;
    private int doorLayer;
    private Door door;

    void Start()
    {
        // "Player" 태그가 있는 오브젝트를 찾아서 플레이어의 Transform을 가져옴
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        inventory = GetComponent<PlayerInventory>();
        interactableLayer = LayerMask.NameToLayer("interactable");
        brazierLayer = LayerMask.NameToLayer("brazier");
        buildingLayer = LayerMask.NameToLayer("building");
        doorLayer = LayerMask.NameToLayer("Door");
        brazier = GetComponent<Brazier>();
    }

    void Update()
    {
        if (Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;

            // 플레이어의 몸통에서 Ray를 발사
            Ray ray = new Ray(playerTransform.position + Vector3.up * 0.1f, playerTransform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            {
 
                int hitLayer = hit.collider.gameObject.layer;

                if (hit.collider.gameObject != curInteractGameobject)
                {
                    curInteractGameobject = hit.collider.gameObject;
                    curInteractable = hit.collider.GetComponent<InteractableObject>(); // 해당 오브젝트에서 InteractableObject 스크립트 가져오기
                    if (hitLayer == interactableLayer)
                    {
                        SetPromptText("[E] " + curInteractable.interactMessage);
                    }
                    if (hitLayer == brazierLayer)
                    {
                        SetPromptText("[F] open brazier");
                    }
                    if (hitLayer == buildingLayer)
                    {
                        SetPromptText("[X] destroy");
                    }
                    if (hitLayer == doorLayer)
                    {
                        door = hit.collider.gameObject.GetComponentInParent<Door>();
                        SetPromptText("[T] open");
                    }
                }
            }
            else
            {
                curInteractGameobject = null;
                curInteractable = null;
                promptText.gameObject.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && curInteractable != null)
        {
            curInteractable.Interact(inventory);
            AudioManager.Instance.PlaySFX(inventory.player.getSound);
            promptText.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (Input.GetKeyDown(KeyCode.F) && curInteractable != null)
            {
                brazier.ToggleBrazier();
            }
            promptText.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.X) && curInteractable != null)
        {
            GameObject res = Instantiate(curInteractable.itemPrefab, curInteractable.dropPositon.transform.position + Vector3.up * 1.5f, Quaternion.identity);
            Rigidbody rb = res.GetComponent<Rigidbody>();
            Collider col = res.GetComponent<Collider>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            if (col != null)
            {
                col.enabled = true;
            }
            Destroy(curInteractGameobject);
            promptText.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.T) /* && curInteractable != null */)
        {
            door.Interact();
            promptText.gameObject.SetActive(false);
        }

    }
    private void SetPromptText(string msg)
    {
        promptText.gameObject.SetActive(true);
        promptText.text = $"<b>{msg}</b>";
    }
}
