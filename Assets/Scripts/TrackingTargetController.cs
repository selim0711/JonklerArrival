using UnityEngine;
using UnityEngine.InputSystem;

public class TrackingTargetController : MonoBehaviour
{
    [SerializeField] private float verticalSensitivity = 2f;
    [SerializeField] private float maxVerticalAngle = 80f; // Maximaler Winkel nach oben/unten

    private float verticalRotation = 0f;
    private InputAction lookAction;

    private void Awake()
    {
        // Initialisiere die InputAction für die Mausbewegung (Look)
        lookAction = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta");
        lookAction.Enable();
    }

    private void OnDestroy()
    {
        // Deaktiviert die InputAction, um Speicher freizugeben
        lookAction.Disable();
    }

    private void Update()
    {
        // Lese den Maus-Input
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        // Berechne die vertikale Rotation (Y-Komponente des Maus-Delta)
        verticalRotation -= lookInput.y * verticalSensitivity * Time.deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);

        // Setze die Rotation des GameObjects in der lokalen X-Achse
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
