using SojaExiles;
using UnityEngine;
using UnityEngine.InputSystem;

public class Key : MonoBehaviour
{
    public string keyID;  // The unique identifier for this key
    private InputAction interactAction;

    private void Start()
    {
        interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        interactAction.performed += AttemptUnlock;
        interactAction.Enable();
    }

    private void OnDestroy()
    {
        if (interactAction != null)
        {
            interactAction.performed -= AttemptUnlock;
            interactAction.Disable();
        }
    }

    private void AttemptUnlock(InputAction.CallbackContext context)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            var door = hit.collider.GetComponent<opencloseDoor>();
            if (door != null && door.isLocked && door.doorID == keyID)
            {
                door.UnlockDoor();
            }
        }
    }
}
