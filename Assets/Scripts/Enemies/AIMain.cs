using UnityEngine;
using UnityEngine.AI;

public class AIMain : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected float health = 100f;
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
    protected Transform player;

    protected PlayerEvents playerEvents;

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

    protected virtual void Start()
    {
        if (GameManager.instance == null) return;
        player = GameManager.instance.playerTransforms[0];
        playerEvents = player.GetComponent<PlayerEvents>();
        playerEvents.onPlayerShoot += OnPlayerShoot;
    }

    protected virtual void Update()
    {
        StateHandler();

        animator.SetFloat("Forward", agent.velocity.magnitude);
    }

    protected virtual void StateHandler()
    {
        AIState oldState = currentState;

        if (health <= 0)
        {
            currentState = AIState.dead;
            aiAttack.DisableColliders();
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

            aiAttack.SetLastKnownPlayerPosition(player.position);

            agent.SetDestination(player.position);
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
        if (health <= 0) return;

        health -= damage;
        hasDamageBeenTaken = true;
    }

    public virtual bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 toPlayer = player.position - transform.position;
        bool isPlayerInFront = Vector3.Angle(transform.forward, toPlayer) < sightAngle / 2;
        bool isPlayerObstructed = Physics.Raycast(transform.position, toPlayer + new Vector3(0, 1, 0), out RaycastHit hit, 
            sightRange, obstructionMask) && hit.transform == player;

        //if (hit.transform != null) print(hit.transform.name + " " + isPlayerInFront + " " + isPlayerObstructed);
        return isPlayerInFront && isPlayerObstructed && toPlayer.sqrMagnitude <= Mathf.Pow(sightRange, 2);
    }

    public virtual bool IsPlayerInSightRange()
    {
        if (player == null) return false;

        return Vector3.SqrMagnitude(transform.position - player.transform.position) <= Mathf.Pow(sightRange, 2);
    }

    public virtual bool IsPlayerInAlertRange()
    {
        if (player == null) return false;

        return Vector3.SqrMagnitude(transform.position - player.transform.position) <= Mathf.Pow(alertRange, 2);
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

    public virtual Transform GetPlayer()
    {
        return player;
    }

    public AIState GetCurrentState()
    {
        return currentState;
    }

    //public float GetRotationSpeed()
    //{
    //    return rotationSpeed;
    //}

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
}