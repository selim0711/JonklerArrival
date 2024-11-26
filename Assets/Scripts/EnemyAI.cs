#define DebugBuild //comment out when game is finished

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum JoinklerFinishers // the finishers animation played when the player dies
{
    uppercut, // default anim
}

public class EnemyAI : MonoBehaviour
{
    private readonly string[] killAnimsStrings = { "KillPlayerAnim_Uppercut" };

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

    private EnemyAnim enemyAnim = null;
    private NavMeshAgent navMeshAgent;
    public bool playerInSight;
    private Vector3 lastKnownNoisePosition;
    private Transform currentPatrolPoint;
    private bool isWaiting;
    private float waitTimer;
    [SerializeField]
    private bool isJoinklerStunned = false;
    private bool hasPlayedStunAnim = false;

    private float joinklerStunnedTimeCurrent = 0.0f;
    private float joinklerStunnedTime = 2f;

    private bool isKillingPlayer = false;
    private bool hasPlayedKillingAnim = false;
    private bool hasKilledPlayer = false;

    private JoinklerFinishers killAnim = JoinklerFinishers.uppercut;

    [SerializeField]
    private GameObject killRoom = null;

    public bool beatboxEvent = false;
    private BoxCollider beatboxEventTrigger = null;

    [SerializeField]
    private bool TestTrigger_Stun = false;
    [SerializeField]
    private bool TestTrigger_Beatbox = false;

    // Neue Variablen für Geräuschgedächtnis
    private float timeSinceLastNoise = 0f;
    [SerializeField] private float noiseMemoryTime = 10f; // Zeit, wie lange Geräusche erinnert werden

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            player.gameObject.GetComponent<PlayerBeatbox>().ActivateEvent(this);
        }
    }

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyAnim = GetComponent<EnemyAnim>();

        navMeshAgent.speed = patrolSpeed;

        var colliders = GetComponents<BoxCollider>();
        foreach (BoxCollider collider in colliders)
        {
            if (collider.isTrigger)
            {
                beatboxEventTrigger = collider;
            }
        }
    }

    private void Update()
    {
        if (isKillingPlayer)
        {
            if (!hasPlayedKillingAnim)
            {
                hasPlayedKillingAnim = true;

                enemyAnim.PlayKillPlayer(killAnimsStrings[(int)killAnim]);

                Debug.Log("Killed Player!");
            }

            if (true) // maybe add an Killscreen later?
            {
                OnFinishKillPLayer();
            }

            return;
        }

#if DebugBuild
        if (TestTrigger_Stun)
        {
            TestTrigger_Stun = false;
            StunJoinkler(4.7f);
        }

        if (TestTrigger_Beatbox)
        {
            TestTrigger_Beatbox = false;
            player.gameObject.GetComponent<PlayerBeatbox>().ActivateEvent(this);
        }
#endif

        if (isJoinklerStunned)
        {
            joinklerStunnedTimeCurrent += Time.deltaTime;
            if (joinklerStunnedTimeCurrent >= joinklerStunnedTime)
            {
                isJoinklerStunned = false;
                navMeshAgent.isStopped = false;
            }
        }

        // Update der Zeit seit dem letzten Geräusch
        if (lastKnownNoisePosition != Vector3.zero)
        {
            timeSinceLastNoise += Time.deltaTime;
            if (timeSinceLastNoise > noiseMemoryTime)
            {
                lastKnownNoisePosition = Vector3.zero;
                timeSinceLastNoise = 0;
            }
        }

        CheckSight();

        if (playerInSight)
        {
            navMeshAgent.speed = pursueSpeed;
            MoveTowards(player.position);
        }
        else if (lastKnownNoisePosition != Vector3.zero)
        {
            navMeshAgent.speed = pursueSpeed;
            MoveTowards(lastKnownNoisePosition);
        }
        else
        {
            navMeshAgent.speed = patrolSpeed;
            Patrol();
        }
    }

    public void StunJoinkler(float stunTime)
    {
        if (!isJoinklerStunned)
        {
            isJoinklerStunned = true;
            navMeshAgent.isStopped = true;
            navMeshAgent.speed = 0;
            joinklerStunnedTime = stunTime;
            joinklerStunnedTimeCurrent = 0;
        }
    }

    public void SetTriggerColliderState(bool value)
    {
        beatboxEventTrigger.isTrigger = value;
    }

    public void KillPlayer(JoinklerFinishers killAnim)
    {
        this.killAnim = killAnim;
        this.isKillingPlayer = true;
    }

    private void OnFinishKillPLayer()
    {
        var GameObjects = (GameObject[]) Object.FindObjectsByType(typeof(GameObject), FindObjectsSortMode.None);

        var killRoomGameObjects = new List<GameObject>();

        for (int i = 0; i < killRoom.transform.childCount; i++)
        {
            killRoomGameObjects.Add(killRoom.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < GameObjects.Length; i++)
        {
            bool invalidObject = false;

            var currentObject = GameObjects[i];

            for (int j = 0; j < killRoomGameObjects.Count; j++)
            {
                if(currentObject == killRoomGameObjects[j])
                {
                    invalidObject = true;
                    break;
                }
            }

            if (invalidObject)
                continue;

            Destroy(currentObject);
        }

        killRoom.SetActive(true);

        //TODO: Trigger Scene restart
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
                RaycastHit[] hits = Physics.RaycastAll(transform.position, directionToPlayer, viewDistance, playerLayer | obstructionLayer);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                    {
                        playerInSight = true;
                        lastKnownNoisePosition = Vector3.zero;
                        break;
                    }
                    else if ((1 << hit.collider.gameObject.layer & obstructionLayer.value) != 0)
                    {
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
            timeSinceLastNoise = 0; // Reset noise memory timer
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
