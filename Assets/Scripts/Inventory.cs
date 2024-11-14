using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    public GameObject inventoryUI;
    public Transform itemSlotParent;
    public GameObject itemSlotPrefab;

    private bool isInventoryOpen = false;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        // Set up the Inventory action
        if (playerInput != null)
        {
            playerInput.actions["Inventory"].performed += ToggleInventory;
        }
    }

    private void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.actions["Inventory"].performed -= ToggleInventory;
        }
    }

    private void Start()
    {
        inventoryUI.SetActive(false);
    }

    private void ToggleInventory(InputAction.CallbackContext context)
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryUI.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            UpdateInventoryUI();
        }
    }

    public void AddItem(Item item)
    {
        items.Add(item);
        if (isInventoryOpen)
        {
            UpdateInventoryUI();
        }
    }

    private void UpdateInventoryUI()
    {
        foreach (Transform child in itemSlotParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Item item in items)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemSlotParent);
            Image icon = slot.transform.GetChild(0).GetComponent<Image>();
            icon.sprite = item.icon;

            Button equipButton = slot.GetComponent<Button>();
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(() => EquipItem(item));
        }
    }

    private void EquipItem(Item item)
    {
        item.isEquipped = !item.isEquipped;
        Debug.Log(item.itemName + (item.isEquipped ? " equipped." : " unequipped."));
    }
}
