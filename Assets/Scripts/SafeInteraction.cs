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
    [SerializeField] private Button closeButton; // Exit-Button hinzufügen
    [SerializeField] private Animator safeDoorAnimator; // Animator der Safe-Tür
    private InputAction openSafeAction; // Referenz zur OpenSafe-Aktion
    public MonoBehaviour movementScript; // Referenz auf das Bewegungsskript des Spielers
    public MonoBehaviour cameraControlScript; // Referenz auf das Kamerasteuerungsskript

    private bool isUnlocked = false;

    private void Awake()
    {
        var playerInput = FindAnyObjectByType<PlayerInput>();

        if (playerInput != null)
            openSafeAction = playerInput.actions["OpenSafe"];
        else
            Debug.LogError("player Input was nullptr while trying to set openAction");

        submitButton.onClick.AddListener(ValidateCode);
        closeButton.onClick.AddListener(CloseSafeUI); // Listener für den Close-Button
        safeUI.SetActive(false);
    }

    private void OnEnable()
    {
        if (!isUnlocked)
        {
            if (openSafeAction != null)
                openSafeAction.performed += _ => ToggleSafeUI();
            else
                Debug.LogError("Open Safe Action was NULL, failed to add ToggleSafeUI!");
        }
    }

    private void OnDisable()
    {
        if (!isUnlocked)
        {
            if (openSafeAction != null)
                openSafeAction.performed -= _ => ToggleSafeUI();
            else
                Debug.LogError("Open Safe Action was NULL, failed to remove ToggleSafeUI!");
        }
    }

    private void ToggleSafeUI()
    {
        if (IsPlayerNear() && !isUnlocked) // Überprüfe auch, ob der Tresor nicht entsperrt ist
        {
            safeUI.SetActive(!safeUI.activeSelf); // Umschalten der Sichtbarkeit des UI
            if (safeUI.activeSelf)
            {
                inputField.text = ""; // Setze das Feld zurück, wenn das UI aktiviert wird
                Cursor.lockState = CursorLockMode.None; // Deaktiviere die Mauseinschränkung
                Cursor.visible = true;
                movementScript.enabled = false; // Deaktiviere das Bewegungsskript
                cameraControlScript.enabled = false; // Deaktiviere das Kamerasteuerungsskript
            }
            else
            {
                CloseSafeUI(); // Stelle sicher, dass das UI und die Einstellungen korrekt zurückgesetzt werden
            }
        }
    }

    public void CloseSafeUI()
    {
        safeUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; // Reaktiviere die Mauseinschränkung
        Cursor.visible = false;
        movementScript.enabled = true; // Aktiviere das Bewegungsskript wieder
        cameraControlScript.enabled = true; // Aktiviere das Kamerasteuerungsskript wieder
    }

    public void ValidateCode()
    {
        string enteredCode = inputField.text;
        const string correctCode = "1234";
        if (enteredCode == correctCode)
        {
            Debug.Log("Code korrekt, Tresor öffnet sich!");
            safeDoorAnimator.SetTrigger("Open"); // Löst die Open-Animation aus
            isUnlocked = true; // Setze den Tresor auf entsperrt
            CloseSafeUI(); // Schließe das UI und setze alles zurück
        }
        else
        {
            Debug.Log("Falscher Code!");
        }
        inputField.text = ""; // Setzt das Input-Feld zurück
    }

    private bool IsPlayerNear()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        return Vector3.Distance(transform.position, player.transform.position) <= 3f;
    }
}
