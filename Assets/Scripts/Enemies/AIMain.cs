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
    protected NetworkVariable<bool> attacking = new NetworkVariable<bool>(false);
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
    protected NetworkVariable<ulong> targetPlayerId = new NetworkVariable<ulong>(default);
    protected Transform targetPlayer;

    protected List<PlayerEvents> playerEvents = new List<PlayerEvents>();

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
    }

    public override void OnNetworkSpawn()
    {
        if (GameManager.Instance == null) return;
        foreach (ulong id in GameManager.Instance.playerIds)
        {
            playerTransforms.Add(NetworkManager.SpawnManager.SpawnedObjects[id].transform.Find("Player").transform);
        }

        foreach (Transform t in playerTransforms)
        {
            if (t == null) continue;

            PlayerEvents playerEvent = t.GetComponent<PlayerEvents>();

            playerEvents.Add(playerEvent);
            playerEvent.onPlayerShoot += OnPlayerShoot;
        }

        currentState.OnValueChanged += (previous, current) =>
        {
            StateChangeHandler(current);
        };

        health.OnValueChanged += (previous, current) =>
        {
            OnHealthChanged();
        };
    }

    protected virtual void Update()
    {
        if (!IsSpawned) return;

        StateHandler();

        animator.SetFloat("Forward", agent.velocity.magnitude);
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void UpdateCurrentStateServerRpc(AIState newState)
    {
        if (currentState.Value == newState) return;

        currentState.Value = newState;
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void UpdateAttackingBoolServerRpc(bool value)
    {
        attacking.Value = value;
    }

    protected virtual void StateHandler()
    {
        if (health.Value <= 0)
        {
            UpdateCurrentStateServerRpc(AIState.dead);
        }
        else if (stunned)
        {
            UpdateCurrentStateServerRpc(AIState.stunned);
        }
        else if ((!hasDamageBeenTaken && !hasPlayerShot) && attacking.Value || CanSeePlayer() || IsPlayerInAlertRange())
        {
            //print("the other one");
            UpdateCurrentStateServerRpc(AIState.attack);

            UpdateAttackingBoolServerRpc(true);
            hasDamageBeenTaken = false;
            hasPlayerShot = false;

            targetPlayer = GetNetworkObject(targetPlayerId.Value).transform.Find("Player");

            aiAttack.SetLastKnownPlayerPosition(targetPlayer.position);

            agent.SetDestination(targetPlayer.position);
        }
        else if (hasDamageBeenTaken || hasPlayerShot)
        {
            print("hasDamageBeenTaken || hasPlayerShot");
            UpdateCurrentStateServerRpc(AIState.attack);

            UpdateAttackingBoolServerRpc(true);
            hasDamageBeenTaken = false;
            hasPlayerShot = false;

            targetPlayer = GetNetworkObject(targetPlayerId.Value).transform.Find("Player");

            aiAttack.SetLastKnownPlayerPosition(targetPlayer.position);

            agent.SetDestination(targetPlayer.position);
        }
        else
        {
            UpdateCurrentStateServerRpc(AIState.wander);
        }
    }

    protected virtual void StateChangeHandler(AIState newState)
    {
        aiWander.enabled = false;
        aiAttack.enabled = false;
        aiDead.enabled = false;

        switch (currentState.Value)
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
                print("enabling attack state");
                break;
            case AIState.dead:
                aiDead.enabled = true;
                break;
        }
    }

    public virtual void TakeDamage(float damage, ulong ownerId)
    {
        if (health.Value <= 0) return;

        SetTargetPlayerIdServerRpc(ownerId);
        //targetPlayer = GetNetworkObject(ownerId).transform.Find("Player");

        TakedamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakedamageServerRpc(float damage)
    {
        health.Value -= damage;
    }

    private void OnHealthChanged()
    {
        hasDamageBeenTaken = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTargetPlayerIdServerRpc(ulong playerId)
    {
        targetPlayerId.Value = playerId;
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

    public virtual bool IsPlayerInSightRange(ulong playerId)
    {
        Transform playerTransform = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId].transform.Find("Player");
        if(Vector3.SqrMagnitude(transform.position - playerTransform.position) <= Mathf.Pow(sightRange, 2))
        {
            if (targetPlayer == null)
            {
                targetPlayer = playerTransform;
            }
            return true;
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

    public virtual void OnPlayerShoot(ulong playerId)
    {
        if (IsPlayerInSightRange(playerId))
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
        return currentState.Value;
    }

    public virtual void SetAttacking(bool value)
    {
        UpdateAttackingBoolServerRpc(value);
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

    public virtual ulong GetTargetPlayerId()
    {
        return targetPlayerId.Value;
    }
}