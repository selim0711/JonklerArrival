using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class Airhorn : MonoBehaviour
{
    public EnemyAI enemyAI; // Referenz zum Enemy AI Skript
    public float airhornIntensity = 30f; // Intensität des Geräuschs
    public AudioSource audioSource; // AudioSource-Komponente
    public AudioClip airhornSound; // Der Airhorn Soundclip
    public bool isThrowable = true; // Ob das Airhorn geworfen werden kann


    private Transform player; // Referenz auf den Spieler
    private Rigidbody rb;
    private Collider airhornCollider;
    private bool isThrown = false; // Ob das Airhorn geworfen wurde
    private InputAction airhornAction; // InputAction für "F_Action"

    private void Start()
    {
        player = GameObject.FindWithTag("PlayerHand")?.transform;
        rb = GetComponent<Rigidbody>();
        airhornCollider = GetComponent<Collider>();

        if (rb != null)
        {
            rb.isKinematic = true; // Standardmäßig deaktiviert
        }



        if (audioSource == null)
        {
            Debug.LogError("Keine AudioSource-Komponente gefunden! Bitte füge eine AudioSource-Komponente hinzu.");
        }

        airhornAction = new InputAction("F_Action", binding: "<Keyboard>/f");
        airhornAction.performed += OnUseAirhorn;
        airhornAction.Enable();
    }

    private void OnDestroy()
    {
        airhornAction.performed -= OnUseAirhorn;
        airhornAction.Disable();
    }

    private void OnUseAirhorn(InputAction.CallbackContext context)
    {
        if (!isThrown && IsChildOfPlayer())
        {
            UseAirhorn();
        }
        else
        {
            Debug.Log("Das Airhorn kann nicht benutzt werden, da es geworfen wurde.");
        }
    }

    public void UseAirhorn()
    {
        PlaySound();
        Debug.Log("Airhorn benutzt!");
        enemyAI?.DetectNoise(transform.position, airhornIntensity);
    }

    public void Throw()
    {
        if (!isThrowable || isThrown)
        {
            Debug.LogWarning("Airhorn kann nicht geworfen werden!");
            return;
        }

        isThrown = true;
        Debug.Log("Airhorn wird geworfen!");

        if (rb != null && airhornCollider != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Stelle auf kontinuierliche Kollisionserkennung um
            Vector3 throwDirection = Camera.main.transform.forward + new Vector3(0, 0.2f, 0); // Leicht nach oben werfen
            rb.AddForce(throwDirection * 10f, ForceMode.Impulse);
            airhornCollider.enabled = true;
        }

        StartCoroutine(TriggerAirhorn());
    }

    private IEnumerator TriggerAirhorn()
    {
        for (int i = 0; i < 5; i++)
        {
            PlaySound();
            enemyAI?.DetectNoise(transform.position, airhornIntensity);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("Airhorn hat das Geräuscherzeugen beendet.");
        MakePickupable();
    }

    private void PlaySound()
    {
        if (audioSource != null && airhornSound != null)
        {
            audioSource.PlayOneShot(airhornSound);
        }
        else
        {
            Debug.LogError("AudioSource oder Airhorn-Soundclip ist nicht korrekt zugewiesen!");
        }
    }

    private void MakePickupable()
    {
        isThrown = false;
        rb.isKinematic = true;
        airhornCollider.enabled = true;
        gameObject.tag = "Item";
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isThrown)
        {
            Debug.Log($"Airhorn kollidiert mit: {collision.gameObject.name}");
            ResetAirhornState();
        }
    }

    private bool IsChildOfPlayer()
    {
        Transform currentParent = transform;

        while (currentParent != null)
        {
            if (currentParent == player)
            {
                return true;
            }
            currentParent = currentParent.parent;
        }

        return false;
    }

    public void ResetAirhornState()
    {
        isThrown = false;
        //rb.isKinematic = true;
        airhornCollider.enabled = true;
        
        Debug.Log("Airhorn zurückgesetzt und nutzbar.");
    }
}
