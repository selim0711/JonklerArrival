using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class Airhorn : MonoBehaviour
{
    public EnemyAI enemyAI; // Referenz zum Enemy AI Skript
    public float airhornIntensity = 30f; // Intensität des Geräuschs
    private InputAction airhornAction; // InputAction für "Airhorn_Action"
    private Transform player; // Referenz auf den Spieler
    public AudioSource audioSource; // AudioSource Komponente
    public AudioClip airhornSound; // Der Airhorn Soundclip

    private void Start()
    {
        // Initialisiere die "Airhorn_Action", die mit einer Taste verknüpft ist
        airhornAction = new InputAction("F_Action", binding: "<Keyboard>/f");
        airhornAction.performed += OnUseAirhorn;
        airhornAction.Enable();

        // Suche nach dem Player in der Szene
        player = GameObject.FindWithTag("PlayerHand")?.transform;

        if (player == null)
        {
            Debug.LogError("Kein Spieler mit dem Tag 'Player' in der Szene gefunden! Bitte stelle sicher, dass der Spieler korrekt getaggt ist.");
        }

        if (enemyAI == null)
        {
            Debug.LogError("Kein EnemyAI Skript zugewiesen! Bitte verknüpfe das EnemyAI Skript im Inspector.");
        }

        if (audioSource == null)
        {
            Debug.LogError("Keine AudioSource Komponente gefunden! Bitte füge eine AudioSource Komponente hinzu und weise sie zu.");
        }
    }

    private void OnDestroy()
    {
        // Entferne das Event, um Speicherlecks zu vermeiden
        airhornAction.performed -= OnUseAirhorn;
    }

    private void OnUseAirhorn(InputAction.CallbackContext context)
    {
        PlaySound();
        Debug.Log("Airhorn benutzt");
        Vector3 airhornPosition = player.position; // Position des Spielers als Ursprung des Geräuschs
        enemyAI.DetectNoise(airhornPosition, airhornIntensity);
    }

    private void PlaySound()
    {
        if (audioSource != null && airhornSound != null)
        {
            audioSource.PlayOneShot(airhornSound);
        }
        else
        {
            Debug.LogError("AudioSource oder Airhorn Soundclip ist nicht korrekt zugewiesen!");
        }
    }
}
