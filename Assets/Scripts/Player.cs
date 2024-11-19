using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

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

    [Header("Noise Settings")]
    [SerializeField] private float walkNoiseIntensity = 5f;
    [SerializeField] private float sprintNoiseIntensity = 9f;
    [SerializeField] private float crouchNoiseIntensity = 1f;
    [SerializeField] private float jumpNoiseIntensity = 15f;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 15f;
    [SerializeField] private float staminaRegenRate = 10f;
    private float currentStamina;

    [Header("UI Elements")]
    [SerializeField] private Slider staminaBar;
    [SerializeField] private CanvasGroup staminaBarCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float fadeOutDelay = 2f;

    [Header("Inventory Settings")]
    [SerializeField] private GameObject inventoryUI; // Das Inventar-UI
    [SerializeField] private TrackingTargetController trackingTargetController; // Referenz zum TrackingTargetController
    private bool isInventoryOpen = false; // Status des Inventars

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private InputAction mouseLockAction;
    private InputAction inventoryAction; 

    private Rigidbody rb;
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;
    private bool isMoving;

    private Coroutine fadeCoroutine;
    private bool isFadingOut = false;

    [SerializeField] private LayerMask enemyLayer;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        currentStamina = maxStamina; // Initialize stamina to max

        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component is missing.");
            return;
        }

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        crouchAction = playerInput.actions["Crouch"];
        mouseLockAction = playerInput.actions["MouseLock"];
        inventoryAction = playerInput.actions["Inventory"];

        // Lock cursor when the game starts
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        jumpAction.performed += OnJump;
        sprintAction.performed += StartSprint;
        sprintAction.canceled += StopSprint;
        crouchAction.performed += StartCrouch;
        crouchAction.canceled += StopCrouch;
        mouseLockAction.performed += ToggleCursorLock;
        inventoryAction.performed += ToggleInventory; // Inventar-Steuerung
    }

    private void OnDisable()
    {
        jumpAction.performed -= OnJump;
        sprintAction.performed -= StartSprint;
        sprintAction.canceled -= StopSprint;
        crouchAction.performed -= StartCrouch;
        crouchAction.canceled -= StopCrouch;
        mouseLockAction.performed -= ToggleCursorLock;
        inventoryAction.performed -= ToggleInventory;
    }

    private void Update()
    {
        if (!isInventoryOpen) // Nur wenn das Inventar geschlossen ist, erlauben wir Bewegung und Kamera
        {
            HandleMovement();
            HandleLook();
        }
        EmitNoiseBasedOnMovement();
        HandleStamina();
        UpdateStaminaUI();
    }
    private void ToggleInventory(InputAction.CallbackContext context)
    {
        isInventoryOpen = !isInventoryOpen;

        // Inventar-UI ein-/ausblenden
        inventoryUI.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            // Maus aktivieren und TrackingTargetController deaktivieren
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (trackingTargetController != null)
                trackingTargetController.enabled = false; // Tracking deaktivieren
        }
        else
        {
            // Maus deaktivieren und TrackingTargetController aktivieren
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (trackingTargetController != null)
                trackingTargetController.enabled = true; // Tracking aktivieren
        }
    }

    private void ToggleCursorLock(InputAction.CallbackContext context)
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float currentSpeed = isCrouching ? crouchSpeed : (isSprinting && currentStamina > 0 ? sprintSpeed : walkSpeed);

        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
        direction = cameraTransform.forward * direction.z + cameraTransform.right * direction.x;
        direction.y = 0f;

        isMoving = moveInput.magnitude > 0;

        Vector3 moveVelocity = direction * currentSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + moveVelocity);
    }

    private void HandleStamina()
    {
        if (isSprinting && isMoving && currentStamina > 0)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

            if (currentStamina <= 0)
            {
                StopSprint(new InputAction.CallbackContext());
            }

            UpdateStaminaUI();
        }
        else if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            UpdateStaminaUI();
        }
    }

    private void UpdateStaminaUI()
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina / maxStamina;
        }
    }

   private void HandleLook()
    {
        Vector2 lookInput = Mouse.current.delta.ReadValue();
        Vector2 lookDelta = lookInput * lookSensitivity * Time.deltaTime;

        transform.Rotate(0f, lookDelta.x, 0f);
        cameraTransform.Rotate(-lookDelta.y, 0f, 0f);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && !isCrouching)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            MakeNoise(jumpNoiseIntensity); // Jumping creates a louder noise
        }
    }

    private void StartSprint(InputAction.CallbackContext context)
    {
        if (currentStamina > 0)
        {
            isSprinting = true;
            ShowStaminaBar();
        }
    }

    private void StopSprint(InputAction.CallbackContext context)
    {
        isSprinting = false;
        HideStaminaBar();
    }

    private void ShowStaminaBar()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeStaminaBar(1f));
    }

    private void HideStaminaBar()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeStaminaBar(0f, fadeOutDelay));
    }

    private IEnumerator FadeStaminaBar(float targetAlpha, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        float startAlpha = staminaBarCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            staminaBarCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        staminaBarCanvasGroup.alpha = targetAlpha;
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
        scale.y = height / standingHeight;
        transform.localScale = scale;
    }

    private void EmitNoiseBasedOnMovement()
    {
        if (moveAction.ReadValue<Vector2>().magnitude > 0) // Only emit noise if moving
        {
            if (isSprinting)
                MakeNoise(sprintNoiseIntensity);
            else if (isCrouching)
                MakeNoise(crouchNoiseIntensity);
            else
                MakeNoise(walkNoiseIntensity);
        }
    }

    private void MakeNoise(float intensity)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, intensity);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent(out EnemyAI enemyAI))
            {
                enemyAI.DetectNoise(transform.position, intensity);
            }
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
