using UnityEngine;

public class KillRoomBehaviour : MonoBehaviour
{
    [SerializeField]
    private Animator  joinklerAnimator = null, cameraAnimator = null;
    void Start()
    {
        var ai = GameObject.FindAnyObjectByType<EnemyAI>();

        ai.killRoom = gameObject;
    }

    public void PlayKillScene()
    {
        
    }
}
