using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections.Generic;

public class AIMain : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] protected EnemyStats enemyStats;
    [SerializeField] protected LayerMask obstructionMask;
    protected NetworkVariable<float> currentHealth = new NetworkVariable<float>();

    protected NetworkVariable<bool> stunned = new NetworkVariable<bool>();
    protected NetworkVariable<bool> attacking = new NetworkVariable<bool>();
    protected NetworkVariable<bool> hasDamageBeenTaken = new NetworkVariable<bool>();
    protected NetworkVariable<bool> hasPlayerShot = new NetworkVariable<bool>();

    protected NavMeshAgent agent;
    protected Rigidbody rb;
    protected Animator animator;

    protected AIWander aiWander;
    protected AIAttack aiAttack;
    protected AIDead aiDead;

    protected List<PlayerEvents> playerEvents = new List<PlayerEvents>();

    protected NetworkVariable<ulong> targetPlayerId = new NetworkVariable<ulong>();
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

        aiWander = GetComponent<AIWander>();
        aiAttack = GetComponent<AIAttack>();
        aiDead = GetComponent<AIDead>();

        currentState.OnValueChanged += (previous, current) => StateChangeHandler();
        targetPlayerId.OnValueChanged += (previous, current) => OnTargetPlayerChanged();
    }

    protected virtual void OnPlayerIdListChange()
    {
        GetNetworkObject(GameManager.Instance.playerIds[GameManager.Instance.playerIds.Count - 1])
            .transform.Find("Player").GetComponent<PlayerEvents>().onPlayerShoot += OnPlayerShoot;
    }

    protected void OnTargetPlayerChanged()
    {
        if (IsServer)
        {
            var targetPlayerNetworkObject = GetNetworkObject(targetPlayerId.Value);
            if (targetPlayerNetworkObject != null)
            {
                print("target found, changing ownership to " + targetPlayerNetworkObject.OwnerClientId);
                GetComponent<NetworkObject>().ChangeOwnership(targetPlayerNetworkObject.OwnerClientId);
            }
            else
            {
                print("no target, removing ownership");
                GetComponent<NetworkObject>().RemoveOwnership();
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.playerIds.OnListChanged += (current) => OnPlayerIdListChange();
        
        if (IsHost)
        {
            currentHealth.Value = enemyStats.health;
        }

        foreach (ulong id in GameManager.Instance.playerIds)
        {
            print("Player Id: " + id + " detected");
            GetNetworkObject(GameManager.Instance.playerIds[GameManager.Instance.playerIds.Count - 1])
                .transform.Find("Player").GetComponent<PlayerEvents>().onPlayerShoot += OnPlayerShoot;
        }

        print(OwnerClientId + " is the owner of " + gameObject.name);
    }

    protected virtual void Update()
    {
        if (!IsSpawned || !IsOwner) return;

        StateHandler();

        animator.SetFloat("Forward", agent.velocity.magnitude);

        if (IsHost)
        {
            if(Input.GetKeyDown(KeyCode.L))
            {
                SetAttackingServerRpc(false);
                //ResetTargetPlayerClientRpc();
            }
        }
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
        else if (stunned.Value)
        {
            if (currentState.Value != AIState.stunned)
            {
                SetCurrentStateServerRpc(AIState.stunned);
            }
        }
        else if (hasDamageBeenTaken.Value || hasPlayerShot.Value || attacking.Value || CanSeePlayer() || IsPlayerInAlertRange())
        {
            if (hasDamageBeenTaken.Value || hasPlayerShot.Value)
            {
                if (IsOwner)
                {
                    SetAgentDestinationServerRpc(targetPlayer.position);
                    aiAttack.SetSearchTimer(0);
                }
            }

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
        aiWander.enabled = false;
        aiAttack.enabled = false;
        aiDead.enabled = false;

        switch (currentState.Value)
        {
            case AIState.idle:
                break;
            case AIState.wander:
                aiWander.enabled = true;
                break;
            case AIState.stunned:
                break;
            case AIState.attack:
                aiAttack.enabled = true;
                break;
            case AIState.dead:
                aiDead.enabled = true;
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
                    SetTargetPlayerIdServerRpc(playerId);
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
                    SetTargetPlayerIdServerRpc(playerId);
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
                    SetTargetPlayerIdServerRpc(playerId);
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
            SetTargetPlayerIdServerRpc(playerId);
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
            SetTargetPlayerIdServerRpc(playerId);
            SetHasPlayerShotServerRpc(true);
        }
    }

    public virtual void EndStun()
    {
        SetStunnedServerRpc(false, 0);
    }

    [ClientRpc]
    protected virtual void SetTargetPlayerTransformClientRpc(ulong playerId)
    {
        targetPlayer = GetNetworkObject(playerId).transform.Find("Player").transform;
    }

    [ClientRpc]
    public virtual void ResetTargetPlayerClientRpc()
    {
        targetPlayer = null;
    }

    [ClientRpc]
    public virtual void SetAgentDestinationClientRpc(Vector3 destination)
    {
        if (!agent.enabled) return;
        agent.SetDestination(destination);
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void ResetTargetPlayerIdServerRpc()
    {
        targetPlayerId.Value = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void SetAgentDestinationServerRpc(Vector3 destination)
    {
        //agent.SetDestination(destination);
        SetAgentDestinationClientRpc(destination);
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void TakeDamageServerRpc(float damage)
    {
        currentHealth.Value -= damage;
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void SetAttackingServerRpc(bool value)
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
    public virtual void SetStunnedServerRpc(bool value, float timer)
    {
        stunned.Value = value;
        if (timer > 0) Invoke(nameof(EndStun), timer);
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SetCurrentStateServerRpc(AIState newState)
    {
        currentState.Value = newState;
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SetTargetPlayerIdServerRpc(ulong playerId)
    {
        print(playerId + " is the new target player id");
        targetPlayerId.Value = playerId;
        SetTargetPlayerTransformClientRpc(playerId);
    }

    public virtual NavMeshAgent GetAgent() { return agent; }
    public virtual EnemyStats GetEnemyStats() { return enemyStats; }
    public virtual Animator GetAnimator() { return animator; }
    public virtual AIState GetCurrentState() { return currentState.Value; }
    public virtual Transform GetTargetPlayer() { return targetPlayer; }
}
