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
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>(); // Add an AudioSource if one isn't assigned
        }
        audioSource.loop = false; // Default to non-looping
        ChangeAudioState(JoinklerAudioStates.Idle); // Start with idle audio
    }

    private void Update()
    {
        HandleAudioState();
    }

    private void HandleAudioState()
    {
        if (enemyAI.playerInSight)
        {
            if (currentState != JoinklerAudioStates.SeenPlayer && currentState != JoinklerAudioStates.Hunting)
            {
                ChangeAudioState(JoinklerAudioStates.SeenPlayer);
            }
            else if (currentState == JoinklerAudioStates.SeenPlayer && !audioSource.isPlaying)
            {
                ChangeAudioState(JoinklerAudioStates.Hunting);
            }
        }
        else if (!enemyAI.playerInSight && (currentState == JoinklerAudioStates.Hunting || currentState == JoinklerAudioStates.SeenPlayer))
        {
            ChangeAudioState(JoinklerAudioStates.LostPlayer);
        }
        else if (currentState == JoinklerAudioStates.LostPlayer && !audioSource.isPlaying)
        {
            ChangeAudioState(JoinklerAudioStates.Idle);
        }
    }

    private void ChangeAudioState(JoinklerAudioStates newState)
    {
        if (currentState == newState) return;

        //Debug.Log($"Transitioning from {currentState} to {newState}");
        currentState = newState;

        if (audioSource.isPlaying) audioSource.Stop();

        AudioClip newClip = GetClipForState(newState);
        if (newClip != null)
        {
            audioSource.clip = newClip;

            audioSource.loop = (newState == JoinklerAudioStates.Hunting);
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"No audio clip assigned for state {newState}");
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
}
