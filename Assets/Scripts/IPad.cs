using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video; // Importiere das Video Namespace

public class IPad : MonoBehaviour
{
    public EnemyAI enemyAI;
    public float iPadIntensity = 30f;
    public bool isThrowable = true;

    private Transform player;
    private Rigidbody rb;
    private Collider iPadCollider;
    private bool isThrown = false;
    private InputAction iPadAction;
    private VideoPlayer videoPlayer;

    private void Awake()
    {
        player = GameObject.FindWithTag("PlayerHand")?.transform;
        rb = GetComponent<Rigidbody>();
        iPadCollider = GetComponent<Collider>();
        videoPlayer = GetComponentInChildren<VideoPlayer>();

        rb.isKinematic = true;

        iPadAction = new InputAction("F_Action", binding: "<Keyboard>/f");
    }

    private void OnEnable()
    {
        iPadAction.performed += ToggleVideoPlay;
        iPadAction.Enable();
    }

    private void OnDisable()
    {
        iPadAction.performed -= ToggleVideoPlay;
        iPadAction.Disable();
    }

    private void ToggleVideoPlay(InputAction.CallbackContext context)
    {
        if (isThrown || !IsChildOfPlayer())
        {
            Debug.Log("Das iPad kann nicht benutzt werden, da es geworfen wurde.");
            return;
        }

        if (videoPlayer && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
        else if (videoPlayer)
        {
            videoPlayer.Play();
            enemyAI?.DetectNoise(transform.position, iPadIntensity);
        }

    }

    public void Throw()
    {
        if (!isThrowable || isThrown)
        {
            Debug.LogWarning("iPad kann nicht geworfen werden!");
            return;
        }

        isThrown = true;
        Debug.Log("iPad wird geworfen!");

        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        Vector3 throwDirection = Camera.main.transform.forward + new Vector3(0, 0.2f, 0);
        rb.AddForce(throwDirection * 10f, ForceMode.Impulse);
        iPadCollider.enabled = true;

        if (videoPlayer && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
    }

    private bool IsChildOfPlayer()
    {
        return player != null && player == transform.parent;
    }
}
