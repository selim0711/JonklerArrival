using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Sight Settings")]
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private float viewAngle = 45f;

    [Header("Hearing Settings")]
    [SerializeField] private float hearingRadius = 15f; // Increase if necessary

    [Header("Detection Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstructionLayer;

    private NavMeshAgent navMeshAgent;
    private bool playerInSight;
    private Vector3 lastKnownNoisePosition;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        CheckSight();
        if (playerInSight)
        {
            MoveTowards(player.position);
        }
        else if (lastKnownNoisePosition != Vector3.zero)
        {
            MoveTowards(lastKnownNoisePosition);
        }
    }

    private void CheckSight()
    {
        playerInSight = false;
        if (player == null) return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < viewAngle / 2)
            {
                RaycastHit[] hits = Physics.RaycastAll(transform.position, directionToPlayer, viewDistance);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                    {
                        playerInSight = true;
                        lastKnownNoisePosition = Vector3.zero;
                        break;
                    }
                    else if ((1 << hit.collider.gameObject.layer & obstructionLayer) != 0)
                    {
                        playerInSight = false;
                        break;
                    }
                }
            }
        }
    }
    /*
    public void DetectNoise(Vector3 noisePosition, float intensity)
    {
        float adjustedHearingRadius = hearingRadius + intensity;
        if (Vector3.Distance(transform.position, noisePosition) <= adjustedHearingRadius)
        {
            lastKnownNoisePosition = noisePosition;

            // Debug log to confirm detection
            Debug.Log("Enemy heard a noise at position: " + noisePosition + " with intensity: " + intensity);

            // Visualize the noise detection position
            Debug.DrawLine(transform.position, noisePosition, Color.yellow, 2f); // Line to noise source
            Debug.DrawRay(noisePosition, Vector3.up * 2f, Color.red, 2f); // Indicator at noise source
        }
    } */

    public void DetectNoise(Vector3 noisePosition, float intensity)
    {
        float detectionRange = hearingRadius + intensity;
        float distanceToNoise = Vector3.Distance(transform.position, noisePosition);

        if (distanceToNoise <= detectionRange)
        {
            lastKnownNoisePosition = noisePosition;
            Debug.Log($"Enemy heard noise at {noisePosition} with intensity {intensity}");
        }
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        navMeshAgent.SetDestination(targetPosition);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 viewAngleA = DirectionFromAngle(-viewAngle / 2);
        Vector3 viewAngleB = DirectionFromAngle(viewAngle / 2);

        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
    }

    private Vector3 DirectionFromAngle(float angleInDegrees)
    {
        angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
