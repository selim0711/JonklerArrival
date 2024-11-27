using UnityEngine;

public class JonklerEndSceneAnim : MonoBehaviour
{
    private Animator jonklerAnim = null;

    [SerializeField, Tooltip("The End position where the Joinkler goes in the Animation")]
    private Transform endKickPosition = null;

    private Vector3 startPosition = Vector3.zero;
    private Vector3 targetVelocity = Vector3.zero;

    [SerializeField, Tooltip("The Time in Seconds it takes for the Joinkler to go from his position to the end kick position")]
    private float kickMoveTime = 10.0f;
    private float currentKickMoveTime = 0.0f;

    private bool hasJumped = false;

    void Start()
    {
        jonklerAnim = GetComponent<Animator>();
    }

    void Update()
    {
        if(hasJumped)
        {
            var positionInterpolate = startPosition + (targetVelocity * currentKickMoveTime);

            currentKickMoveTime += Time.deltaTime;

            if(currentKickMoveTime >= kickMoveTime)
            {
                hasJumped = false;

                transform.position = startPosition + (targetVelocity * kickMoveTime);

                PlayKnockOutCamera();
            }
            else
            {
                transform.position = positionInterpolate;
            }
        }
    }

    public void OnJonklerJumped()
    {
        hasJumped = true;

        startPosition = transform.position;
        targetVelocity = (endKickPosition.position - transform.position) / kickMoveTime;

        jonklerAnim.SetBool("spin", true);
    }

    private void PlayKnockOutCamera()
    {
        Debug.Log("Knocking out Camera");
    }
}
