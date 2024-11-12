using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Sight Settings")]
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private float viewAngle = 45f;

    [Header("Hearing Settings")]
    [SerializeField] private float hearingRadius = 15f;

    [Header("Detection Settings")]
    [SerializeField] public Transform player;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstructionLayer;

    [Header("Patrol Settings")]
    [SerializeField] private List<Transform> patrolPoints;
    [SerializeField] private float waitTimeAtPatrolPoint = 2f;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float pursueSpeed = 6f;

    private NavMeshAgent navMeshAgent;
    public bool playerInSight;
    private Vector3 lastKnownNoisePosition;
    private Transform currentPatrolPoint;
    private bool isWaiting;
    private float waitTimer;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = patrolSpeed; // Set initial speed to patrol speed
    }

    private void Update()
    {
        CheckSight();

        if (playerInSight)
        {
            // Pursue the player if in sight
            navMeshAgent.speed = pursueSpeed;
            MoveTowards(player.position);
        }
        else if (lastKnownNoisePosition != Vector3.zero)
        {
            // Move towards the last known noise position if heard
            navMeshAgent.speed = pursueSpeed;
            MoveTowards(lastKnownNoisePosition);
        }
        else
        {
            // Patrol if no player in sight or noise detected
            navMeshAgent.speed = patrolSpeed;
            Patrol();
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

    private void Patrol()
    {
        if (navMeshAgent.remainingDistance <= 0.5f && !isWaiting)
        {
            // When reaching a patrol point, start waiting
            isWaiting = true;
            waitTimer = waitTimeAtPatrolPoint;
            navMeshAgent.ResetPath();
        }

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                ChooseRandomPatrolPoint();
            }
        }
    }

    private void ChooseRandomPatrolPoint()
    {
        // Select a random patrol point from the list, avoiding the current point
        Transform nextPatrolPoint;
        do
        {
            nextPatrolPoint = patrolPoints[Random.Range(0, patrolPoints.Count)];
        } while (nextPatrolPoint == currentPatrolPoint);

        currentPatrolPoint = nextPatrolPoint;
        navMeshAgent.SetDestination(currentPatrolPoint.position);
    }

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
