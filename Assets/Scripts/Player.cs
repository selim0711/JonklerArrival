using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private Transform cameraTransform;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();

        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component is missing.");
            return;
        }

        // Initialize input actions
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        crouchAction = playerInput.actions["Crouch"];
    }

    private void OnEnable()
    {
        jumpAction.performed += OnJump;
        sprintAction.performed += StartSprint;
        sprintAction.canceled += StopSprint;
        crouchAction.performed += StartCrouch;
        crouchAction.canceled += StopCrouch;
    }

    private void OnDisable()
    {
        jumpAction.performed -= OnJump;
        sprintAction.performed -= StartSprint;
        sprintAction.canceled -= StopSprint;
        crouchAction.performed -= StartCrouch;
        crouchAction.canceled -= StopCrouch;
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);

        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
        direction = cameraTransform.forward * direction.z + cameraTransform.right * direction.x;
        direction.y = 0f;

        Vector3 moveVelocity = direction * currentSpeed * Time.deltaTime;
        transform.Translate(moveVelocity, Space.World);
    }

    private void HandleLook()
    {
        Vector2 lookInput = Mouse.current.delta.ReadValue();
        Vector2 lookDelta = lookInput * lookSensitivity * Time.deltaTime;

        transform.Rotate(0f, lookDelta.x, 0f); // Horizontal rotation for player
        cameraTransform.Rotate(-lookDelta.y, 0f, 0f); // Vertical rotation for camera
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && !isCrouching) // Prevent jumping while crouched
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void StartSprint(InputAction.CallbackContext context)
    {
        isSprinting = true;
    }

    private void StopSprint(InputAction.CallbackContext context)
    {
        isSprinting = false;
    }

    private void StartCrouch(InputAction.CallbackContext context)
    {
        isCrouching = true;
        AdjustHeight(crouchHeight);
    }

    private void StopCrouch(InputAction.CallbackContext context)
    {
        isCrouching = false;
        AdjustHeight(standingHeight);
    }

    private void AdjustHeight(float height)
    {
        Vector3 scale = transform.localScale;
        scale.y = height / standingHeight; // Adjust the scale proportionally
        transform.localScale = scale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
