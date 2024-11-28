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

    [SerializeField] private VideoClip[] videoClips; // Array für Videos
    private int currentVideoIndex = 0; // Index des aktuellen Videos

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
        iPadAction.performed += PlayNextVideo;
        iPadAction.Enable();
    }

    private void OnDisable()
    {
        iPadAction.performed -= PlayNextVideo;
        iPadAction.Disable();
    }

    private void PlayNextVideo(InputAction.CallbackContext context)
    {
        if (isThrown || !IsChildOfPlayer())
        {
            Debug.Log("Das iPad kann nicht benutzt werden, da es geworfen wurde.");
            return;
        }

        if (videoPlayer && videoClips.Length > 0)
        {
            currentVideoIndex = (currentVideoIndex + 1) % videoClips.Length; // Zum nächsten Video wechseln
            videoPlayer.clip = videoClips[currentVideoIndex]; // Setze das nächste Video
            videoPlayer.Play(); // Spiele das Video ab
            enemyAI?.DetectNoise(transform.position, iPadIntensity);
            Debug.Log($"Video {currentVideoIndex + 1} von {videoClips.Length} abgespielt.");
        }
        else
        {
            Debug.LogWarning("Keine Videos verfügbar!");
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
