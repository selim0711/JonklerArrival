using UnityEngine;
public enum JoinklerAudioStates
{
    Idle,
    SeenPlayer,
    Hunting,
    Hunting2,
    SwitchState,
}
public class EnemySoundstate : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] stateClips;

    private JoinklerAudioStates nextPlayingState = JoinklerAudioStates.Idle;

    private bool changeState = false;
    private bool hasPlayedAudioSwitch = false;

    EnemyAI enemyAi = null;
    AudioSource currentAudioSource = null;
    void Start()
    {
        enemyAi = GetComponent<EnemyAI>();
        currentAudioSource = GetComponent<AudioSource>();

        currentAudioSource.loop = false;

        ChangeAudioState(JoinklerAudioStates.Idle);
    }

    public void ChangeAudioState(JoinklerAudioStates state)
    {
        changeState = true;
        nextPlayingState = state;
        hasPlayedAudioSwitch = false;
    }

    private void UpdateSoundState()
    {
        if (changeState)
        {
            if (!hasPlayedAudioSwitch)
            {
                if (currentAudioSource.isPlaying)//Skip execution of clip till done
                    return;

                hasPlayedAudioSwitch = true;
            }
            else
            {
                if (!currentAudioSource.isPlaying)
                {
                    changeState = false;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //enemyAi.playerInSight;
    }
}
