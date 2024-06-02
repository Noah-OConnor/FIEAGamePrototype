using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.Netcode;

public class AIMain : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected NetworkVariable<float> health = new NetworkVariable<float>(100f);
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float rotationSpeed = 5f;
    [SerializeField] protected float sightAngle = 45f;

    [SerializeField] protected float sightRange = 10f;
    [SerializeField] protected float alertRange = 5f;

    [SerializeField] protected LayerMask obstructionMask;

    // Flags
    protected bool hasPlayerShot = false;
    protected bool hasDamageBeenTaken = false;
    protected bool attacking = false;
    protected bool stunned = false;

    // Components
    protected NavMeshAgent agent;
    protected Rigidbody rb;
    protected Animator animator;

    // States
    protected AIWander aiWander;
    protected AIAttack aiAttack;
    protected AIDead aiDead;

    // References
    protected List<Transform> playerTransforms = new List<Transform>();
    protected Transform targetPlayer;

    protected List<PlayerEvents> playerEvents = new List<PlayerEvents>();

    [SerializeField] protected AIState currentState = AIState.idle;
    public enum AIState
    {
        idle,
        wander,
        stunned,
        attack,
        dead
    }

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        aiWander = GetComponent<AIWander>();
        aiAttack = GetComponent<AIAttack>();
        aiDead = GetComponent<AIDead>();
    }

    public override void OnNetworkSpawn()
    {
        if (GameManager.Instance == null) return;
        foreach (ulong id in GameManager.Instance.playerIds.Value)
        {
            print(NetworkManager.SpawnManager.SpawnedObjects[id].transform.Find("Player").transform.name);
            playerTransforms.Add(NetworkManager.SpawnManager.SpawnedObjects[id].transform.Find("Player").transform);
        }

        foreach (Transform t in playerTransforms)
        {
            if (t == null) continue;

            PlayerEvents playerEvent = t.GetComponent<PlayerEvents>();

            playerEvents.Add(playerEvent);
            playerEvent.onPlayerShoot += OnPlayerShoot;
        }
    }

    protected virtual void Update()
    {
        StateHandler();

        animator.SetFloat("Forward", agent.velocity.magnitude);
    }

    protected virtual void StateHandler()
    {
        AIState oldState = currentState;

        if (health.Value <= 0)
        {
            currentState = AIState.dead;
        }
        else if (stunned)
        {
            currentState = AIState.stunned;
        }
        else if ((!hasDamageBeenTaken && !hasPlayerShot) && attacking || CanSeePlayer() || IsPlayerInAlertRange())
        {
            currentState = AIState.attack;

            attacking = true;
            hasDamageBeenTaken = false;
            hasPlayerShot = false;
        }
        else if (hasDamageBeenTaken || hasPlayerShot)
        {
            currentState = AIState.attack;

            attacking = true;
            hasDamageBeenTaken = false;
            hasPlayerShot = false;

            aiAttack.SetLastKnownPlayerPosition(targetPlayer.position);

            agent.SetDestination(targetPlayer.position);
        }
        else
        {
            currentState = AIState.wander;
        }

        if (oldState != currentState)
        {
            aiWander.enabled = false;
            aiAttack.enabled = false;
            aiDead.enabled = false;

            switch (currentState)
            {
                case AIState.idle:
                    // not sure if we need this
                    break;
                case AIState.wander:
                    aiWander.enabled = true;
                    break;
                case AIState.stunned:
                    // not sure if we need this
                    break;
                case AIState.attack:
                    aiAttack.enabled = true;
                    break;
                case AIState.dead:
                    aiDead.enabled = true;
                    break;
            }
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (health.Value <= 0) return;

        //health.Value -= damage;
        hasDamageBeenTaken = true;

        // damage the enemy through an rpc
        TakedamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakedamageServerRpc(float damage)
    {
        health.Value -= damage;
    }

    public virtual bool CanSeePlayer()
    {
        if (playerTransforms == null) return false;

        foreach(Transform player in playerTransforms)
        {
            if (player == null) continue;

            Vector3 toPlayer = player.transform.position - transform.position;
            bool isPlayerInFront = Vector3.Angle(transform.forward, toPlayer) < sightAngle / 2;
            bool isPlayerObstructed = Physics.Raycast(transform.position, toPlayer + new Vector3(0, 1, 0), out RaycastHit hit, 
                sightRange, obstructionMask) && hit.transform == player;

            if (isPlayerInFront && isPlayerObstructed && toPlayer.sqrMagnitude <= Mathf.Pow(sightRange, 2))
            {
                if (targetPlayer == null)
                {
                    targetPlayer = player.transform;
                }

                return true;
            }
        }
        return false;
    }

    public virtual bool IsPlayerInSightRange()
    {
        if (playerTransforms == null) return false;

        foreach (Transform player in playerTransforms)
        {
            if (player == null) continue;

            if(Vector3.SqrMagnitude(transform.position - player.transform.position) <= Mathf.Pow(sightRange, 2))
            {
                if (targetPlayer == null)
                {
                    targetPlayer = player.transform;
                }
                return true;
            }
        }
        return false;
    }

    public virtual bool IsPlayerInAlertRange()
    {
        if (playerTransforms == null) return false;

        foreach (Transform player in playerTransforms)
        {
            if (Vector3.SqrMagnitude(transform.position - player.transform.position) <= Mathf.Pow(alertRange, 2))
            {
                if (targetPlayer == null)
                {
                    targetPlayer = player.transform;
                }
                return true;
            }
        }
        return false;
    }

    public virtual void OnPlayerShoot()
    {
        if (IsPlayerInSightRange())
        {
            hasPlayerShot = true;
        }
    }

    public virtual NavMeshAgent GetAgent()
    {
        return agent;
    }

    public virtual Rigidbody GetRigidbody()
    {
        return rb;
    }

    public virtual Animator GetAnimator()
    {
        return animator;
    }

    public virtual List<Transform> GetPlayer()
    {
        return playerTransforms;
    }

    public AIState GetCurrentState()
    {
        return currentState;
    }

    public virtual void SetAttacking(bool value)
    {
        attacking = value;
    }

    public virtual void SetStunned(float length)
    {
        stunned = true;
        Invoke(nameof(EndStun), length);
    }

    protected virtual void EndStun()
    {
        stunned = false;
    }

    public virtual void SetTargetPlayer(Transform player)
    {
        targetPlayer = player;
    }

    public virtual Transform GetTargetPlayer()
    {
        return targetPlayer;
    }
}