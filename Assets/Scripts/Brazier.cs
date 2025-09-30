using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brazier : MonoBehaviour
{
    [System.Serializable]
    public class BrazierSlot
    {
        public ItemData item;
        public int quantity;
    }
    public GameObject braizerUI;
    public PlayerInventory inventory;
    public ItemSlotUI[] uiSlots;
    private bool isBrazierOpen = false;
    // Start is called before the first frame update
    void Start()
    {
        braizerUI.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ToggleBrazier()
    {
        isBrazierOpen = !isBrazierOpen;
        braizerUI.SetActive(isBrazierOpen);
        Cursor.lockState = isBrazierOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isBrazierOpen;
    }
}
