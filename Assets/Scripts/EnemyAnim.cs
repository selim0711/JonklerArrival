using UnityEngine;
using UnityEngine.AI;

public class EnemyAnim : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Berechne die Geschwindigkeit aus der Bewegungsgeschwindigkeit des NavMeshAgent
        float speed = navMeshAgent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        // Setze IsRunning basierend auf der aktuellen Geschwindigkeit des NavMeshAgent
        bool isRunning = navMeshAgent.speed > 3.5f;  // Nehme an, dass Werte über 3,5 ein Verfolgen anzeigen
        animator.SetBool("IsRunning", isRunning);

        // Setze isStunned basierend auf dem Zustand des NavMeshAgent
        bool isStunned = navMeshAgent.isStopped;
        animator.SetBool("isStunned", isStunned);
    }

    public void PlayKillPlayer(string KillAnim)
    {
        animator.SetBool("isKillingPlayer", true);
        animator.SetTrigger(KillAnim);
    }
}
