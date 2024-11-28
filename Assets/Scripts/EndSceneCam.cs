using UnityEngine;

public class EndSceneCam : MonoBehaviour
{
    public KillRoomBehaviour killRoomBehaviour = null;

    private Animator camAnimator = null;

    private bool updateVignette = false;

    [SerializeField]
    private float updateSpeed = 0.1f;
    public float updateSpeedCurrent = 0.0f;

    private void Start()
    {
        camAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (updateVignette)
        {
            const float updateSpeedEnd = 1.0f;

            updateSpeedCurrent += updateSpeed * Time.deltaTime;

            if(updateSpeedCurrent > updateSpeed)
            {
                updateVignette = false;

                updateSpeedCurrent = updateSpeedEnd;
            }

            killRoomBehaviour.UpdateVignette(updateSpeedCurrent);
        }
        
    }
    private void OnAnimationPleadTriggerJoinkler()
    {
        killRoomBehaviour.ActivateJoinkler();
    }

    private void OnAnimationVignetteFade()
    {
        killRoomBehaviour.ActivateVignette();
        updateVignette = true;
    }
}
