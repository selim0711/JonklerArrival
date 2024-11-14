using UnityEngine;
using UnityEngine.InputSystem;

public class ItemRaycastPickup : MonoBehaviour
{
    [SerializeField] private Inventory inventory; // Reference to the Inventory
    [SerializeField] private float pickupRange = 2f; // Pickup range in units
    private PlayerInput playerInput; // Reference to PlayerInput

    private void Awake()
    {
        // PlayerInput holen und Pickup Action binden
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            // Binding f�r die Pickup-Action mit der OnPickup-Methode verkn�pfen
            playerInput.actions["Interact"].performed += OnPickup;
           

        }
    }

    private void OnDestroy()
    {
        // Event-Abonnement beim Zerst�ren entfernen, um Memory Leaks zu vermeiden
        if (playerInput != null)
        {
            playerInput.actions["Interact"].performed -= OnPickup;
        }
    }

    private void OnPickup(InputAction.CallbackContext context)
    {
        // Nur wenn der Button gedr�ckt wurde, die Aktion ausf�hren
        if (context.performed)
        {
            TryPickupItem();
        }
    }

    private void TryPickupItem()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            if (hit.collider.CompareTag("Item"))
            {
                ItemPickupData itemData = hit.collider.GetComponent<ItemPickupData>();
                if (itemData != null)
                {
                    inventory.AddItem(itemData.itemData);
                    Destroy(hit.collider.gameObject);
                    Debug.Log("Item aufgenommen: " + itemData.itemData.itemName);
                }
            }
        }
    }
}
