using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class Airhorn : MonoBehaviour
{
    public EnemyAI enemyAI; // Referenz zum Enemy AI Skript
    public float airhornIntensity = 30f; // Intensit�t des Ger�uschs
    public float triggerDuration = 5f; // Dauer des Ger�uscherzeugens
    public AudioSource audioSource; // AudioSource-Komponente
    public AudioClip airhornSound; // Der Airhorn Soundclip
    public bool isThrowable = true; // Ob das Airhorn geworfen werden kann
    private Transform player; // Referenz auf den Spieler

    private Rigidbody rb;
    private Collider airhornCollider;
    private bool isThrown = false; // Ob das Airhorn geworfen wurde
    private InputAction airhornAction; // InputAction f�r "F_Action"

    private void Start()
    {
        player = GameObject.FindWithTag("PlayerHand")?.transform;

        // Physik-Initialisierung
        rb = GetComponent<Rigidbody>();
        airhornCollider = GetComponent<Collider>();

        if (rb != null)
        {
            rb.isKinematic = true; // Standardm��ig deaktiviert
        }

        if (audioSource == null)
        {
            Debug.LogError("Keine AudioSource-Komponente gefunden! Bitte f�ge eine AudioSource-Komponente hinzu.");
        }

        // Input f�r "F_Action" (Hupen in der Hand)
        airhornAction = new InputAction("F_Action", binding: "<Keyboard>/f");
        airhornAction.performed += OnUseAirhorn;
        airhornAction.Enable();
    }

    private void OnDestroy()
    {
        // Input-Aktion deaktivieren, um Speicherlecks zu vermeiden
        airhornAction.performed -= OnUseAirhorn;
        airhornAction.Disable();
    }

    private void OnUseAirhorn(InputAction.CallbackContext context)
    {
        if (!isThrown && IsChildOfPlayer()) // Nur in der Hand nutzbar
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

        // Physik aktivieren
        if (rb != null && airhornCollider != null)
        {
            rb.isKinematic = false; // Physik aktivieren
            rb.AddForce(Camera.main.transform.forward * 10f, ForceMode.VelocityChange);
            airhornCollider.enabled = true; // Collider aktivieren
        }

        // Ger�uscherzeugung starten
        StartCoroutine(TriggerAirhorn());
    }

    private IEnumerator TriggerAirhorn()
    {
        for (int i = 0; i < 5; i++) // 5 Mal Ger�usche abspielen
        {
            PlaySound();
            enemyAI?.DetectNoise(transform.position, airhornIntensity);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("Airhorn hat das Ger�uscherzeugen beendet.");
        MakePickupable(); // Airhorn erneut aufhebbar machen
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
        isThrown = false; // Status zur�cksetzen
        rb.isKinematic = true; // Physik deaktivieren
        airhornCollider.enabled = true; // Collider aktivieren
        gameObject.tag = "Item"; // Tag setzen, damit es aufhebbar ist
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isThrown)
        {
            Debug.Log($"Airhorn kollidiert mit: {collision.gameObject.name}");
            rb.isKinematic = true; // Bewegung stoppen
            airhornCollider.enabled = true; // Collider aktivieren
            ResetAirhornState();
        }
    }

    private bool IsChildOfPlayer()
    {
        // Pr�fe die gesamte Parent-Hierarchie
        Transform currentParent = transform;

        while (currentParent != null)
        {
            if (currentParent == player)
            {
                return true; // Airhorn ist Teil der Spieler-Hierarchie
            }
            currentParent = currentParent.parent; // Gehe eine Ebene h�her
        }

        return false; // Airhorn ist nicht Teil des Spielers
    }

    public void ResetAirhornState()
    {
        isThrown = false; // Werfen zur�cksetzen
        rb.isKinematic = true; // Physik deaktivieren
        airhornCollider.enabled = true; // Collider aktivieren
        Debug.Log("Airhorn zur�ckgesetzt und nutzbar.");
    }
}
