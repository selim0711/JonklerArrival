using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class FlashLight : MonoBehaviour
{
    public GameObject flashlightLight; // Die Lichtquelle (Child der Taschenlampe)
    private bool isFlashlightOn = false; // Status der Taschenlampe
    private InputAction flashlightAction; // InputAction f�r "F_Action"
    private Transform player; // Referenz auf den Spieler

    private void Start()
    {
        // Initialisiere die "F_Action", die mit der Taste F verkn�pft ist
        flashlightAction = new InputAction("F_Action", binding: "<Keyboard>/f");
        flashlightAction.performed += OnToggleFlashlight;
        flashlightAction.Enable();

        // Suche nach dem Player in der Szene
        player = GameObject.FindWithTag("PlayerHand")?.transform;

        if (player == null)
        {
            Debug.LogError("Kein Spieler mit dem Tag 'Player' in der Szene gefunden! Bitte stelle sicher, dass der Spieler korrekt getaggt ist.");
        }

        // Pr�fe, ob die Lichtquelle zugewiesen wurde
        if (flashlightLight == null)
        {
            Debug.LogError("Keine Lichtquelle zugewiesen! Bitte die Lichtquelle im Inspector verkn�pfen.");
        }
    }

    private void OnDestroy()
    {
        // Entferne das Event, um Speicherlecks zu vermeiden
        flashlightAction.performed -= OnToggleFlashlight;
    }

    private void OnToggleFlashlight(InputAction.CallbackContext context)
    {
        // Pr�fe, ob die Taschenlampe Teil der Spieler-Hierarchie ist
        if (!IsChildOfPlayer())
        {
            Debug.LogWarning("Taschenlampe kann nicht gesteuert werden, da sie kein Teil des Spielers ist.");
            return;
        }

        // Umschalten der Taschenlampe
        if (!isFlashlightOn)
        {
            StartCoroutine(TurnOnFlashlight());
        }
        else
        {
            StartCoroutine(TurnOffFlashlight());
        }
    }

    IEnumerator TurnOnFlashlight()
    {
        Debug.Log("Taschenlampe einschalten");
        flashlightLight.SetActive(true); // Licht aktivieren
        isFlashlightOn = true;
        yield return null; // Keine Verz�gerung ben�tigt
    }

    IEnumerator TurnOffFlashlight()
    {
        Debug.Log("Taschenlampe ausschalten");
        flashlightLight.SetActive(false); // Licht deaktivieren
        isFlashlightOn = false;
        yield return null; // Keine Verz�gerung ben�tigt
    }

    private bool IsChildOfPlayer()
    {
        // Pr�fe die gesamte Parent-Hierarchie
        Transform currentParent = transform;

        while (currentParent != null)
        {
            if (currentParent == player)
            {
                return true; // Taschenlampe ist Teil der Spieler-Hierarchie
            }
            currentParent = currentParent.parent; // Gehe eine Ebene h�her
        }

        return false; // Taschenlampe ist nicht Teil des Spielers
    }
}

