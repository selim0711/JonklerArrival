using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float lookSensitivity = 2f; // Adjust this value to control sensitivity
    [SerializeField] private Transform cameraTransform;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isSprinting;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();

        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            lookAction = playerInput.actions["Look"];
            jumpAction = playerInput.actions["Jump"];
            sprintAction = playerInput.actions["Sprint"];
        }
        else
        {
            Debug.LogError("PlayerInput component is missing.");
        }
    }

    private void OnEnable()
    {
        if (jumpAction != null)
            jumpAction.performed += OnJump;

        if (sprintAction != null)
        {
            sprintAction.performed += ctx => isSprinting = true;
            sprintAction.canceled += ctx => isSprinting = false;
        }
    }

    private void OnDisable()
    {
        if (jumpAction != null)
            jumpAction.performed -= OnJump;

        if (sprintAction != null)
        {
            sprintAction.performed -= ctx => isSprinting = true;
            sprintAction.canceled -= ctx => isSprinting = false;
        }
    }

    private void Update()
    {
        if (moveAction != null) HandleMovement();
        if (lookAction != null) HandleLook();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
        direction = cameraTransform.forward * direction.z + cameraTransform.right * direction.x;
        direction.y = 0f;

        Vector3 moveVelocity = direction * currentSpeed * Time.deltaTime;
        transform.Translate(moveVelocity, Space.World);
    }

    private void HandleLook()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        Vector2 lookDelta = lookInput * lookSensitivity * Time.deltaTime;

        transform.Rotate(0f, lookDelta.x, 0f); // Horizontal rotation for player
        cameraTransform.Rotate(-lookDelta.y, 0f, 0f); // Vertical rotation for camera
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
