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
    [SerializeField] private Button closeButton; // Exit-Button hinzuf�gen
    [SerializeField] private Animator safeDoorAnimator; // Animator der Safe-T�r
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
        closeButton.onClick.AddListener(CloseSafeUI); // Listener f�r den Close-Button
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
        if (IsPlayerNear() && !isUnlocked) // �berpr�fe auch, ob der Tresor nicht entsperrt ist
        {
            safeUI.SetActive(!safeUI.activeSelf); // Umschalten der Sichtbarkeit des UI
            if (safeUI.activeSelf)
            {
                inputField.text = ""; // Setze das Feld zur�ck, wenn das UI aktiviert wird
                Cursor.lockState = CursorLockMode.None; // Deaktiviere die Mauseinschr�nkung
                Cursor.visible = true;
                movementScript.enabled = false; // Deaktiviere das Bewegungsskript
                cameraControlScript.enabled = false; // Deaktiviere das Kamerasteuerungsskript
            }
            else
            {
                CloseSafeUI(); // Stelle sicher, dass das UI und die Einstellungen korrekt zur�ckgesetzt werden
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

    private bool IsPlayerNear()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        return Vector3.Distance(transform.position, player.transform.position) <= 3f;
    }
}
