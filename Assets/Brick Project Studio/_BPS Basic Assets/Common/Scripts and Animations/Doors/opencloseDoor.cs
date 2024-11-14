using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SojaExiles

{
	public class opencloseDoor : MonoBehaviour
	{
        public Animator openandclose;
        public bool open = false;
        public Transform player;
        public float interactionRange = 3f;

        private InputAction interactAction;

        private void Start()
        {
            // Initialize the Interact action to listen for the "E" key press
            interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
            interactAction.performed += OnInteract;
            interactAction.Enable();
        }

        private void OnDestroy()
        {
            interactAction.performed -= OnInteract;
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            // Raycast from the Player's position forward
            Ray ray = new Ray(player.position, player.forward);
            RaycastHit hit;

            // Check if the ray hits within the interaction range and hits this door
            if (Physics.Raycast(ray, out hit, interactionRange))
            {
                if (hit.transform == transform) // Ensure the raycast is hitting this specific door
                {
                    Debug.Log("Door detected. Attempting to open/close.");
                    if (!open)
                    {
                        StartCoroutine(Opening());
                    }
                    else
                    {
                        StartCoroutine(Closing());
                    }
                }
            }
            else
            {
                Debug.Log("No interactable object within range.");
            }
        }

        IEnumerator Opening()
        {
            Debug.Log("Opening the door");
            openandclose.Play("Opening");
            open = true;
            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator Closing()
        {
            Debug.Log("Closing the door");
            openandclose.Play("Closing");
            open = false;
            yield return new WaitForSeconds(0.5f);
        }
    }
}