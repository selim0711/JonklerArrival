using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class SafeInteraction : MonoBehaviour
{
    [SerializeField] private GameObject safeUI;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeButton; // Exit-Button
    [SerializeField] private Animator safeDoorAnimator; // Animator der Safe-T�r
    [SerializeField] private TextMeshProUGUI interactionText; // Interaction-Text
    [SerializeField] private GameObject[] objectsToToggle; // GameObjects, die aktiviert/deaktiviert werden sollen
    [SerializeField] private float raycastDistance = 3f; // Reichweite des Raycasts
    [SerializeField] private Camera playerCamera; // Spieler-Kamera

    private InputAction openSafeAction; // Referenz zur OpenSafe-Aktion
    public MonoBehaviour movementScript; // Bewegungsskript des Spielers
    public MonoBehaviour cameraControlScript; // Kamerasteuerungsskript

    private bool isUnlocked = false;

    private void Awake()
    {
        var playerInput = FindObjectOfType<PlayerInput>();
        openSafeAction = playerInput.actions["OpenSafe"];

        submitButton.onClick.AddListener(ValidateCode);
        closeButton.onClick.AddListener(CloseSafeUI); // Listener f�r den Close-Button
        safeUI.SetActive(false);
        interactionText.gameObject.SetActive(false); // Interaction-Text initial ausblenden
    }

    private void OnEnable()
    {
        if (!isUnlocked)
        {
            openSafeAction.performed += _ => ToggleSafeUI();
        }
    }

    private void OnDisable()
    {
        if (!isUnlocked)
        {
            openSafeAction.performed -= _ => ToggleSafeUI();
        }
    }

    private void Update()
    {
        HandleInteractionText(); // �berpr�ft, ob der Interaction-Text angezeigt werden soll
    }

    private void HandleInteractionText()
    {
        // �berpr�ft, ob der Spieler mit einem Raycast auf den Safe schaut
        if (IsPlayerLookingAtSafe() && !safeUI.activeSelf && !isUnlocked)
        {
            interactionText.gameObject.SetActive(true); // Zeigt den Interaction-Text an
            interactionText.text = "Press E to interact"; // Setzt den Text
        }
        else
        {
            interactionText.gameObject.SetActive(false); // Versteckt den Interaction-Text
        }
    }

    private void ToggleSafeUI()
    {
        // �berpr�ft, ob der Spieler mit einem Raycast auf den Safe schaut
        if (IsPlayerLookingAtSafe() && !isUnlocked)
        {
            safeUI.SetActive(!safeUI.activeSelf); // Umschalten der Sichtbarkeit des UI

            if (safeUI.activeSelf)
            {
                inputField.text = ""; // Setze das Feld zur�ck
                Cursor.lockState = CursorLockMode.None; // Deaktiviere die Mauseinschr�nkung
                Cursor.visible = true;
                movementScript.enabled = false; // Deaktiviere das Bewegungsskript
                cameraControlScript.enabled = false; // Deaktiviere das Kamerasteuerungsskript

                // Deaktiviert zus�tzliche GameObjects
                foreach (var obj in objectsToToggle)
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                CloseSafeUI(); // Stelle sicher, dass alles zur�ckgesetzt wird
            }
        }
    }

    public void CloseSafeUI()
    {
        safeUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; // Reaktiviere die Mauseinschr�nkung
        Cursor.visible = false;
        movementScript.enabled = true; // Aktiviere das Bewegungsskript wieder
        cameraControlScript.enabled = true; // Aktiviere das Kamerasteuerungsskript wieder

        // Aktiviert zus�tzliche GameObjects wieder
        foreach (var obj in objectsToToggle)
        {
            obj.SetActive(true);
        }
    }

    public void ValidateCode()
    {
        string enteredCode = inputField.text;
        const string correctCode = "1234";
        if (enteredCode == correctCode)
        {
            Debug.Log("Code korrekt, Tresor �ffnet sich!");
            safeDoorAnimator.SetTrigger("Open"); // L�st die Open-Animation aus
            isUnlocked = true; // Setze den Tresor auf entsperrt
            CloseSafeUI(); // Schlie�e das UI und setze alles zur�ck
        }
        else
        {
            Debug.Log("Falscher Code!");
        }
        inputField.text = ""; // Setzt das Input-Feld zur�ck
    }

    private bool IsPlayerLookingAtSafe()
    {
        // Raycast von der Spieler-Kamera aus nach vorne
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red); // Debug-Ray anzeigen

        // �berpr�ft, ob der Raycast den Safe trifft
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
        {
            return hit.collider.gameObject == gameObject; // True, wenn der Safe getroffen wurde
        }
        return false;
    }
}
