using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.Services.Lobbies.Models;

public class AIAttack : MonoBehaviour
{
    [SerializeField] protected float meleeRange = 2f;
    [SerializeField] protected float searchDuration = 10f;
    [SerializeField] protected float searchRadius = 5f;
    [SerializeField] protected float searchSpeed = 5f;
    [SerializeField] protected float chaseSpeed = 8f;
    [SerializeField] protected float chaseRotationSpeed = 8f;

    protected Vector3 lastKnownPlayerPosition;

    protected bool resetting = false;
    protected bool attacking = false;

    protected AIMain aiMain;
    protected Animator animator;
    protected NavMeshAgent agent;
    protected Transform player;

    [SerializeField] protected AttackState currentState;
    protected enum AttackState
    {
        none,
        chase,
        attack,
        search
    }

    protected virtual void OnEnable()
    {
        aiMain = GetComponent<AIMain>();
        animator = aiMain.GetAnimator();
        agent = aiMain.GetAgent();
        player = aiMain.GetPlayer();
        currentState = AttackState.none;
    }

    protected virtual void Update()
    {
        StateHandler();
    }

    protected virtual void StateHandler()
    {
        AttackState oldState = currentState;

        if (IsPlayerInMeleeRange() || attacking)
        {
            currentState = AttackState.attack;
        }
        else if (aiMain.CanSeePlayer() || aiMain.IsPlayerInAlertRange())
        {
            currentState = AttackState.chase;
        }
        else
        {
            currentState = AttackState.search;
        }

        if (oldState != currentState)
        {
            CancelInvoke(nameof(Chase));
            switch (currentState)
            {
                case AttackState.chase:
                    InvokeRepeating(nameof(Chase), 0f, 0.1f);
                    agent.stoppingDistance = 0f;
                    break;
                case AttackState.search:
                    lastKnownPlayerPosition = player.position;
                    StartCoroutine(SearchRoutine());
                    agent.stoppingDistance = 0f;
                    break;
                case AttackState.attack:
                    //agent.SetDestination(transform.position);
                    agent.stoppingDistance = meleeRange;
                    Attack();
                    attacking = true;
                    break;
            }
        }
    }

    protected virtual void Attack()
    {
        animator.SetTrigger("Attack");
        Invoke(nameof(ResetAttack), 2f);
    }

    protected virtual void ResetAttack()
    {
        attacking = false;
    }

    protected virtual void Chase()
    {
        if (currentState != AttackState.chase) return;

        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    protected virtual IEnumerator SearchRoutine()
    {
        float searchTime = 0f;
        agent.speed = searchSpeed;

        // Move to the last known player position
        agent.SetDestination(lastKnownPlayerPosition);

        while (searchTime < searchDuration)
        {
            if (currentState != AttackState.search)
            {
                yield break;
            }

            // If the enemy has reached the last known player position, make it wander around that position
            if (agent.remainingDistance < 0.5f)
            {
                Vector3 randomDirection = Random.insideUnitSphere * searchRadius;
                randomDirection += lastKnownPlayerPosition;
                NavMeshHit hit;
                NavMesh.SamplePosition(randomDirection, out hit, searchRadius, 1);
                agent.SetDestination(hit.position);
            }

            searchTime += Time.deltaTime;
            yield return null;
        }

        aiMain.SetAttacking(false);
    }

    public virtual bool IsPlayerInMeleeRange()
    {
        return Vector3.SqrMagnitude(transform.position - player.position) <= Mathf.Pow(meleeRange, 2);
    }
}
