using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("ending ui")]
    public GameObject endingUIPanel;
    private player player;
    private Vector3 initialPosition;
    private PlayerInventory inventory;
    private bool isDead = false;
    void Start()
    {
        player = GetComponent<player>();
        inventory = GetComponent<PlayerInventory>();
        endingUIPanel.gameObject.SetActive(false);
        initialPosition = player.transform.position;
    }

    void Update()
    {
        if (isDead)
        {
            return;
        }
        if (player.health.curValue <= 0)
        {
            isDead = true;
            HandleDeath();
        }
    }
    void HandleDeath()
    {
        ShowEndingUI();
    }
    void ShowEndingUI()
    {
        if (endingUIPanel != null)
        {
            endingUIPanel.SetActive(true);
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void OnClick_ResetAll()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("StartScene");
    }
    public void OnClick_ResetInventoryOnly()
    {
        inventory.ClearInventory();
        if (inventory.equipped != null)
        {
            Destroy(inventory.equipped);
            inventory.equipped = null;
        }
        if (inventory.weared != null)
        {
            Destroy(inventory.weared);
            inventory.weared = null;
        }
        player.health.curValue = player.health.startValue;
        player.hunger.curValue = player.hunger.startValue;
        player.attackDamage = player.basicAttackDamage;
        player.woodAttack = player.basicAttackDamage;
        player.stoneAttack = player.basicAttackDamage;
        transform.position = initialPosition;
        if (endingUIPanel != null)
        {
            endingUIPanel.SetActive(false);
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isDead = false;
    }
}
