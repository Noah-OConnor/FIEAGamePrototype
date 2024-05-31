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
    protected Animator animator;

    private Coroutine wanderRoutine;

    protected virtual void OnEnable()
    {
        aiMain = GetComponent<AIMain>();
        agent = aiMain.GetAgent();
        animator = aiMain.GetAnimator();
        agent.enabled = true;
        agent.speed = wanderSpeed;
        agent.stoppingDistance = 0f;

        wanderRoutine = StartCoroutine(WanderRoutine());
    }

    //protected virtual void Update()
    //{
    //    if (agent.velocity.magnitude < 0.05f) 
    //    {
    //        animator.SetFloat("Strafe", 0);
    //        animator.SetFloat("Forward", 0);
    //        return;
    //    }

    //    Vector3 movementDirection = agent.velocity.normalized;
    //    float strafe = Vector3.Dot(transform.right, movementDirection) * agent.velocity.magnitude;
    //    float forward = Vector3.Dot(transform.forward, movementDirection * agent.velocity.magnitude);

    //    animator.SetFloat("Strafe", Mathf.Lerp(animator.GetFloat("Strafe"), strafe, 0.05f));
    //    animator.SetFloat("Forward", Mathf.Lerp(animator.GetFloat("Forward"), forward, 0.05f));
    //    Quaternion newRotation = Quaternion.LookRotation(agent.velocity.normalized, Vector3.up);
        
    //    // lerp from current rotation to new rotation
    //    transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * aiMain.GetRotationSpeed());
    //}

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