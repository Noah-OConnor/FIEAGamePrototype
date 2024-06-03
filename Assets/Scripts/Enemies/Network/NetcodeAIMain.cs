using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections.Generic;

public class NetcodeAIMain : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] protected EnemyStats enemyStats;
    [SerializeField] protected LayerMask obstructionMask;
    protected NetworkVariable<float> currentHealth = new NetworkVariable<float>();

    protected bool stunned;
    protected NetworkVariable<bool> attacking = new NetworkVariable<bool>();
    protected NetworkVariable<bool> hasDamageBeenTaken = new NetworkVariable<bool>();
    protected NetworkVariable<bool> hasPlayerShot = new NetworkVariable<bool>();

    protected NavMeshAgent agent;
    protected Rigidbody rb;
    protected Animator animator;

    protected NetcodeAIWander aiWander;
    protected NetcodeAIAttack aiAttack;
    protected NetcodeAIDead aiDead;

    protected List<PlayerEvents> playerEvents = new List<PlayerEvents>();

    protected Transform targetPlayer;

    [SerializeField] protected NetworkVariable<AIState> currentState = new NetworkVariable<AIState>(AIState.idle);
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

        aiWander = GetComponent<NetcodeAIWander>();
        aiAttack = GetComponent<NetcodeAIAttack>();
        aiDead = GetComponent<NetcodeAIDead>();

        currentState.OnValueChanged += (previous, current) => StateChangeHandler();
    }

    protected virtual void OnPlayerIdListChange()
    {
        GetNetworkObject(GameManager.Instance.playerIds[GameManager.Instance.playerIds.Count - 1])
            .transform.Find("Player").GetComponent<PlayerEvents>().onPlayerShoot += OnPlayerShoot;
    }

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.playerIds.OnListChanged += (current) => OnPlayerIdListChange();
        currentHealth.Value = enemyStats.health;

        foreach (ulong id in GameManager.Instance.playerIds)
        {
            print("Player Id: " + id + " detected");
            GetNetworkObject(GameManager.Instance.playerIds[GameManager.Instance.playerIds.Count - 1])
                .transform.Find("Player").GetComponent<PlayerEvents>().onPlayerShoot += OnPlayerShoot;
        }
    }

    protected virtual void Update()
    {
        if (!IsSpawned) return;

        StateHandler();

        animator.SetFloat("Forward", agent.velocity.magnitude);
    }

    protected virtual void StateHandler()
    {
        if (currentHealth.Value <= 0)
        {
            if (currentState.Value != AIState.dead)
            {
                SetCurrentStateServerRpc(AIState.dead);
            }
        }
        else if (stunned)
        {
            if (currentState.Value != AIState.stunned)
            {
                SetCurrentStateServerRpc(AIState.stunned);
            }
        }
        else if (hasDamageBeenTaken.Value || hasPlayerShot.Value || attacking.Value || CanSeePlayer() || IsPlayerInAlertRange())
        {
            if (currentState.Value != AIState.attack)
            {
                SetCurrentStateServerRpc(AIState.attack);
                SetAttackingServerRpc(true);
            }

            SetHasPlayerShotServerRpc(false);
            SetHasDamageBeenTakenServerRpc(false);
        }
        else
        {
            if (currentState.Value != AIState.wander)
            {
                SetCurrentStateServerRpc(AIState.wander);
            }
        }
    }

    protected virtual void StateChangeHandler()
    {
        //aiWander.enabled = false;
        //aiAttack.enabled = false;
        //aiDead.enabled = false;

        switch (currentState.Value)
        {
            case AIState.idle:
                break;
            case AIState.wander:
                //aiWander.enabled = true;
                break;
            case AIState.stunned:
                break;
            case AIState.attack:
                //aiAttack.enabled = true;
                break;
            case AIState.dead:
                //aiDead.enabled = true;
                break;
        }
    }

    public virtual bool IsPlayerInSightRange()
    {
        foreach (ulong playerId in GameManager.Instance.playerIds)
        {
            Transform playerTransform = GetNetworkObject(playerId).transform.Find("Player").transform;
            if (Vector3.SqrMagnitude(transform.position - playerTransform.position) <= Mathf.Pow(enemyStats.sightRange, 2))
            {
                if (targetPlayer == null)
                {
                    targetPlayer = playerTransform;
                }
                return true;
            }
        }
        return false;
    }

    public virtual bool IsPlayerInAlertRange()
    {
        foreach (ulong playerId in GameManager.Instance.playerIds)
        {
            Transform playerTransform = GetNetworkObject(playerId).transform.Find("Player").transform;
            if (Vector3.SqrMagnitude(transform.position - playerTransform.position) <= Mathf.Pow(enemyStats.alertRange, 2))
            {
                if (targetPlayer == null)
                {
                    targetPlayer = playerTransform;
                }
                return true;
            }
        }
        return false;
    }

    public virtual bool CanSeePlayer()
    {
        foreach (ulong playerId in GameManager.Instance.playerIds)
        {
            Transform playerTransform = GetNetworkObject(playerId).transform.Find("Player").transform;

            Vector3 toPlayer = playerTransform.position - transform.position;
            bool isPlayerInFront = Vector3.Angle(transform.forward, toPlayer) < enemyStats.sightAngle / 2;
            bool isPlayerObstructed = Physics.Raycast(transform.position, toPlayer + new Vector3(0, 1, 0), out RaycastHit hit,
                enemyStats.sightRange, obstructionMask) && hit.transform != playerTransform;

            if (isPlayerInFront && !isPlayerObstructed && toPlayer.sqrMagnitude <= Mathf.Pow(enemyStats.sightRange, 2))
            {
                if (targetPlayer == null)
                {
                    targetPlayer = playerTransform;
                }
                return true;
            }
        }
        return false;
    }

    public virtual void TakeDamage(float damage, ulong playerId)
    {
        if (currentHealth.Value <= 0) return;

        if (targetPlayer == null)
        {
            targetPlayer = GetNetworkObject(playerId).transform.Find("Player").transform;
        }

        TakeDamageServerRpc(damage);
        SetHasDamageBeenTakenServerRpc(true);
    }

    public virtual void OnPlayerShoot(ulong playerId)
    {
        if (targetPlayer != null) return;

        Transform playerTransform = GetNetworkObject(playerId).transform.Find("Player").transform;
        if (Vector3.SqrMagnitude(transform.position - playerTransform.position) <= Mathf.Pow(enemyStats.sightRange, 2))
        {
            targetPlayer = playerTransform;
            SetHasPlayerShotServerRpc(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void TakeDamageServerRpc(float damage)
    {
        currentHealth.Value -= damage;
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SetAttackingServerRpc(bool value)
    {
        attacking.Value = value;
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SetHasPlayerShotServerRpc(bool value)
    {
        hasPlayerShot.Value = value;
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SetHasDamageBeenTakenServerRpc(bool value)
    {
        hasDamageBeenTaken.Value = value;
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SetCurrentStateServerRpc(AIState newState)
    {
        currentState.Value = newState;
    }
}
