using UnityEngine;

public class TrackingTargetController : MonoBehaviour
{
    [SerializeField] private float verticalSensitivity = 2f;
    private float verticalRotation = 0f;

    private void Update()
    {
        // Get the vertical input from the mouse
        float verticalInput = Input.GetAxis("Mouse Y");

        // Adjust the vertical rotation based on input
        verticalRotation -= verticalInput * verticalSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f); // Limits vertical rotation

        // Apply rotation to the Tracking Target
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
