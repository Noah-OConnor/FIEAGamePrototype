using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AIWander : MonoBehaviour
{
    [SerializeField] protected Vector2 wanderIntervalRange;
    [SerializeField] protected float wanderRadius;
    [SerializeField] protected float wanderSpeed;

    protected AIMain aiMain;
    protected NavMeshAgent agent;

    private Coroutine wanderRoutine;

    protected virtual void OnEnable()
    {
        aiMain = GetComponent<AIMain>();
        agent = aiMain.GetAgent();
        agent.speed = wanderSpeed;
        wanderRoutine = StartCoroutine(WanderRoutine());
    }

    protected virtual void OnDisable()
    {
        if (wanderRoutine != null)
        {
            StopCoroutine(wanderRoutine);
            wanderRoutine = null;
        }
    }

    protected virtual IEnumerator WanderRoutine()
    {
        while (true)
        {
            float wanderInterval = Random.Range(wanderIntervalRange.x, wanderIntervalRange.y);
            yield return new WaitForSeconds(wanderInterval);
            if (aiMain.GetCurrentState() != AIMain.AIState.wander) yield break;

            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
            Vector3 finalPosition = hit.position;

            agent.SetDestination(finalPosition);

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                yield return null;
            }
        }
    }
}
