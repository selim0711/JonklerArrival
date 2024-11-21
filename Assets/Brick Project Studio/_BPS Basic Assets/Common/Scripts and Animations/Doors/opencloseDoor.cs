using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SojaExiles

{
    public class opencloseDoor : MonoBehaviour
    {
        public Animator openandclose;  // Animator that controls the door animations
        public string doorID = "";  // Door ID for key matching, empty means unlocked by default
        public bool open = false;  // Current state of the door
        public Transform player;  // Reference to the player transform
        public float interactionRange = 3f;  // Maximum distance to interact with the door
        public bool isLocked => !string.IsNullOrEmpty(doorID);  // Locked if doorID is not empty

        private InputAction interactAction;

        private void Start()
        {
            interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
            interactAction.performed += OnInteract;
            interactAction.Enable();
        }

        private void OnDestroy()
        {
            if (interactAction != null)
            {
                interactAction.performed -= OnInteract;
                interactAction.Disable();
            }
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            if (Vector3.Distance(player.position, transform.position) <= interactionRange)
            {
                if (!isLocked || open)
                {
                    ToggleDoor();
                }
                else
                {
                    Debug.Log("The door is locked.");
                }
            }
        }

        private void ToggleDoor()
        {
            if (!open)
            {
                StartCoroutine(Opening());
            }
            else
            {
                StartCoroutine(Closing());
            }
        }

        IEnumerator Opening()
        {
            openandclose.Play("Opening");
            open = true;
            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator Closing()
        {
            openandclose.Play("Closing");
            open = false;
            yield return new WaitForSeconds(0.5f);
        }

        public void UnlockDoor()
        {
            doorID = "";  // Remove the door ID to unlock it
            Debug.Log("Door unlocked");
        }
    }
}
