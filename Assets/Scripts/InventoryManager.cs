using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryUI; // Das UI, das ein-/ausgeblendet wird
    public Transform handTransform; // Referenz auf die Hand des Spielers
    public Button[] inventorySlots; // Feste Buttons f�r die 12 Slots
    private GameObject currentHeldItem; // Das aktuell gehaltene Item

    public MonoBehaviour movementScript; // Referenz auf das Bewegungsskript des Spielers

    private bool isInventoryOpen = false;
    private List<Item> inventory = new List<Item>(); // Liste der Items (mit Icon und Namen)
    private Item equippedItem; // Aktuell ausger�stetes Item

    public InputAction pickupAction; // F�r das Aufheben von Items
    public InputAction inventoryAction; // Input f�r das Inventar �ffnen
    private InputAction throwAction;

    private void Awake()
    {
        pickupAction = new InputAction("Pickup", binding: "<Keyboard>/e");
        inventoryAction = new InputAction("Inventory", binding: "<Keyboard>/i");
        throwAction = new InputAction("Throw_Action", binding: "<Keyboard>/e");

        pickupAction.performed += ctx => Interact(); // Diese Methode behandelt das Aufheben
        inventoryAction.performed += ctx => ToggleInventory();
        throwAction.performed += ctx => ThrowEquippedItem();
    }
    private void OnEnable()
    {
        pickupAction?.Enable();
        inventoryAction?.Enable();
        throwAction?.Enable();
    }

    private void OnDisable()
    {
        pickupAction.Disable();
        inventoryAction.Disable();
        throwAction.Disable();
    }

    private void Interact()
    {
        // Pr�fen, ob ein Item in der N�he ist
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 2f))
        {
            if (hit.collider.CompareTag("Item"))
            {
                Item item = hit.collider.GetComponent<Item>(); // Hole das Item-Skript
                if (item == null) return;

                if (inventory.Count < inventorySlots.Length)
                {
                    AddItem(item); // F�ge das Item ins Inventar hinzu
                    item.gameObject.SetActive(false); // Deaktiviere das Item in der Welt
                    Debug.Log($"Item {item.itemName} aufgehoben!");
                }
                else
                {
                    Debug.Log("Inventar ist voll!");
                }
            }
        }
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryUI.SetActive(isInventoryOpen);

        // Bewegungsskript aktivieren/deaktivieren
        if (movementScript != null)
        {
            movementScript.enabled = !isInventoryOpen;
        }

        Debug.Log(isInventoryOpen ? "Inventar ge�ffnet" : "Inventar geschlossen");
    }

    public void AddItem(Item item)
    {
        inventory.Add(item);
        Debug.Log($"{item.itemName} wurde dem Inventar hinzugef�gt.");
        UpdateInventoryUI(); // Aktualisiere das Inventar-UI
    }

    public void EquipItem(Item item)
    {
        if (equippedItem == item)
        {
            // Wenn das Item bereits ausger�stet ist, r�ste es ab
            UnequipItem();
            
            return;
        }

        // Falls ein anderes Item bereits ausger�stet ist, r�ste es ab
        if (currentHeldItem != null)
        {
            currentHeldItem.SetActive(false); // Deaktiviere das aktuell gehaltene Item
            
        }

        // Aktiviere das neue Item in der Hand
        currentHeldItem = item.gameObject;
        currentHeldItem.SetActive(true);
        currentHeldItem.transform.SetParent(handTransform);
        currentHeldItem.transform.localPosition = Vector3.zero; // Setze Position relativ zur Hand
        currentHeldItem.transform.localRotation = Quaternion.identity; // Setze Rotation
        currentHeldItem.GetComponent<Collider>().enabled = false; // Deaktiviere den Collider
        currentHeldItem.GetComponent<Rigidbody>().isKinematic = true; // Deaktiviere Physik
        equippedItem = item;

        Debug.Log($"{item.itemName} wurde ausger�stet!");
        
    }

    public void UnequipItem()
    {
        if (currentHeldItem != null)
        {
            currentHeldItem.SetActive(false); // Deaktiviere das aktuelle Item
            currentHeldItem.transform.SetParent(null); // Entferne die Parent-Verbindung
        }
        
        equippedItem = null;
        Debug.Log("Item wurde abger�stet!");
    }

    private void ThrowEquippedItem()
    {
        if (equippedItem == null || !equippedItem.GetComponent<Airhorn>().isThrowable)
        {
            Debug.Log("Kein ausger�stetes Item oder Item ist nicht werfbar.");
            return;
        }

        Debug.Log($"Werfe Item: {equippedItem.itemName}");

        // Entferne das Item aus der Hand und dem Inventar
        GameObject thrownItem = equippedItem.gameObject;
        inventory.Remove(equippedItem);
        UnequipItem();

        // Aktivere Physik und l�se es von der Hand
        thrownItem.SetActive(true);
        thrownItem.transform.SetParent(null);
        Rigidbody rb = thrownItem.GetComponent<Rigidbody>() ?? thrownItem.AddComponent<Rigidbody>();
        rb.isKinematic = false;

        // Werfe das Item
        thrownItem.GetComponent<Airhorn>()?.Throw();
        

        // Aktualisiere die UI
        UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
    {
        Debug.Log("UpdateInventoryUI wird aufgerufen");
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < inventory.Count)
            {
                Debug.Log($"Slot {i}: Setze Item {inventory[i].itemName}");

                // Setze den Text
                var textComponent = inventorySlots[i].GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = inventory[i].itemName; // Setze den Itemnamen
                }

                // Setze das Icon
                var iconImage = inventorySlots[i].GetComponentInChildren<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = inventory[i].itemIcon; // Setze das Item-Icon
                    iconImage.enabled = true;
                }

                // F�ge die Klick-Logik hinzu
                inventorySlots[i].onClick.RemoveAllListeners(); // Entferne alte Listener
                int index = i; // Wichtige Kopie f�r den Lambda-Ausdruck
                inventorySlots[i].onClick.AddListener(() =>
                {
                    if (equippedItem == inventory[index])
                    {
                        UnequipItem(); // Abr�sten, wenn bereits ausger�stet
                    }
                    else
                    {
                        EquipItem(inventory[index]); // Ausr�sten
                    }
                });
                inventorySlots[i].interactable = true; // Mach den Slot aktiv
            }
            else
            {
                var textComponent = inventorySlots[i].GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = ""; // Leerer Text
                }

                var iconImage = inventorySlots[i].GetComponentInChildren<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = null; // Entferne das Icon
                    iconImage.enabled = false;
                }

                inventorySlots[i].onClick.RemoveAllListeners(); // Entferne Listener
                inventorySlots[i].interactable = false; // Mach den Slot inaktiv
            }
        }
    }
}
