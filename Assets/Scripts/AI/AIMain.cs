using UnityEngine;
using UnityEngine.AI;

public class AIMain : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float health = 100f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float rotationSpeed = 5f;

    // Components
    private NavMeshAgent agent;
    private Rigidbody rb;
    private Animator animator;

    // States
    private AIWander aiWander;
    private AIAttack aiAttack;
    private AIDead aiDead;

    private AIState currentState = AIState.idle;
    private enum AIState
    {
        idle,
        wandering,
        stunned,
        attacking,
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

    protected virtual void Update()
    {
        StateHandler();
    }

    private void StateHandler()
    {
        AIState oldState = currentState;

        if (health <= 0)
        {
            currentState = AIState.dead;
        }
        // attack else if
        else
        {
            currentState = AIState.wandering;
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
                case AIState.wandering:
                    aiWander.enabled = true;
                    break;
                case AIState.stunned:
                    // not sure if we need this
                    break;
                case AIState.attacking:
                    aiWander.enabled = true;
                    break;
                case AIState.dead:
                    aiWander.enabled = true;
                    break;
            }
        }
    }

    public NavMeshAgent GetAgent()
    {
        return agent;
    }

    public Rigidbody GetRigidbody()
    {
        return rb;
    }

    public Animator GetAnimator()
    {
        return animator;
    }
}
