using UnityEngine;
public enum JoinklerAudioStates
{
    Idle,
    SeenPlayer,
    Hunting,
    LostPlayer,
}
public class EnemySoundstate : MonoBehaviour
{
    [Header("Audio Clips for Each State")]
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip seenPlayerClip;
    [SerializeField] private AudioClip huntingClip;
    [SerializeField] private AudioClip lostPlayerClip;

    [SerializeField] private AudioSource audioSource;

    private JoinklerAudioStates currentState = JoinklerAudioStates.Idle;
    private EnemyAI enemyAI;

    private void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        audioSource.loop = false; // Default to non-looping
        ChangeAudioState(JoinklerAudioStates.Idle); // Start with idle audio
    }

    private void Update()
    {
        // Check enemy AI state and update sound accordingly
        if (enemyAI.playerInSight && currentState != JoinklerAudioStates.SeenPlayer && currentState != JoinklerAudioStates.Hunting)
        {
            ChangeAudioState(JoinklerAudioStates.SeenPlayer);
        }
        else if (!enemyAI.playerInSight && (currentState == JoinklerAudioStates.SeenPlayer || currentState == JoinklerAudioStates.Hunting))
        {
            ChangeAudioState(JoinklerAudioStates.LostPlayer);
        }

        UpdateSoundState();
    }

    private void ChangeAudioState(JoinklerAudioStates newState)
    {
        currentState = newState;

        // Stop the current audio clip if it's playing
        if (audioSource.isPlaying)
            audioSource.Stop();

        // Select the appropriate audio clip based on the state
        AudioClip newClip = GetClipForState(newState);
        if (newClip != null)
        {
            audioSource.clip = newClip;

            // Enable looping only if the state is Hunting
            audioSource.loop = (newState == JoinklerAudioStates.Hunting);

            audioSource.Play();
        }
    }

    private AudioClip GetClipForState(JoinklerAudioStates state)
    {
        switch (state)
        {
            case JoinklerAudioStates.Idle:
                return idleClip;
            case JoinklerAudioStates.SeenPlayer:
                return seenPlayerClip;
            case JoinklerAudioStates.Hunting:
                return huntingClip;
            case JoinklerAudioStates.LostPlayer:
                return lostPlayerClip;
            default:
                return null;
        }
    }

    private void UpdateSoundState()
    {
        // If the clip finishes, transition to the continuous state as needed
        if (!audioSource.isPlaying && !audioSource.loop)
        {
            if (currentState == JoinklerAudioStates.SeenPlayer)
            {
                ChangeAudioState(JoinklerAudioStates.Hunting); // Switch to hunting after player is seen
            }
            else if (currentState == JoinklerAudioStates.LostPlayer)
            {
                ChangeAudioState(JoinklerAudioStates.Idle); // Return to idle if player is lost
            }
        }
    }
}
