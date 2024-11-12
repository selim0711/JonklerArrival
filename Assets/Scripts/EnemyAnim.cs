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
        float speed = navMeshAgent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        // Set IsRunning based on the current NavMeshAgent speed
        bool isRunning = navMeshAgent.speed > 3.5f; // Assume >3.5 indicates pursuing
        animator.SetBool("IsRunning", isRunning);
    }
}
