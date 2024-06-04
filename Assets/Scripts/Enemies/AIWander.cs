using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AIWander : MonoBehaviour
{
    protected AIMain aiMain;
    protected NavMeshAgent agent;
    protected EnemyStats enemyStats;

    private Coroutine wanderRoutine;

    protected virtual void OnEnable()
    {
        aiMain = GetComponent<AIMain>();
        agent = aiMain.GetAgent();
        enemyStats = aiMain.GetEnemyStats();
        agent.enabled = true;
        agent.stoppingDistance = 0.5f;

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
            agent.speed = enemyStats.wanderSpeed;
            float wanderInterval = Random.Range(enemyStats.wanderIntervalRange.x, enemyStats.wanderIntervalRange.y);
            yield return new WaitForSeconds(wanderInterval);
            if (aiMain.GetCurrentState() != AIMain.AIState.wander) yield break;

            Vector3 randomDirection = Random.insideUnitSphere * enemyStats.wanderRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, enemyStats.wanderRadius, 1);
            Vector3 finalPosition = hit.position;

            agent.SetDestination(finalPosition);

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                yield return null;
            }
        }
    }
}