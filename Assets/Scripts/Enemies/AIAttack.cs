using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AIAttack : MonoBehaviour
{
    [SerializeField] protected float searchDuration = 10f;
    [SerializeField] protected float searchRadius = 5f;
    [SerializeField] protected float searchSpeed = 5f;
    [SerializeField] protected float chaseSpeed = 8f;
    [SerializeField] protected float rotationSpeed = 8f;

    protected float summonTimer = 0f;
    protected float searchTime;
    protected Vector3 lastKnownPlayerPosition;
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

    [SerializeField] protected EnemyWeaponStats rightWeapon;
    [SerializeField] protected EnemyWeaponStats leftWeapon;

    [SerializeField] protected Transform arrowSpawnTransform;
    [SerializeField] protected Transform arrowPrefab;
    [SerializeField] protected Transform magicSpawnTransform;
    [SerializeField] protected Transform magicPrefab;

    [SerializeField] protected EnemyWeaponStats unarmedWeapon;

    protected virtual void OnEnable()
    {
        aiMain = GetComponent<AIMain>();
        animator = aiMain.GetAnimator();
        agent = aiMain.GetAgent();
        player = aiMain.GetPlayer();
        currentState = AttackState.none;

        animator.SetBool("Combat", true);

        if (rightWeapon == null)
        {
            rightWeapon = unarmedWeapon;
        }

        if (leftWeapon == null)
        {
            leftWeapon = unarmedWeapon;
        }
    }

    protected virtual void OnDisable()
    {
        currentState = AttackState.none;
        CancelInvoke(nameof(Chase));
        CancelInvoke(nameof(SpinMove));
        CancelInvoke(nameof(ShootCrossbow));
        CancelInvoke(nameof(ShootMagic));

        animator.SetBool("Combat", false);
    }

    protected virtual void Update()
    {
        StateHandler();
        summonTimer += Time.deltaTime;
    }

    protected virtual void StateHandler()
    {
        AttackState oldState = currentState;

        if (IsPlayerInRange(rightWeapon.rangedRange) || IsPlayerInRange(rightWeapon.meleeRange) || attacking)
        {
            currentState = AttackState.attack;

            FacePlayer();
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
            animator.SetBool("Blocking", false);
            animator.SetBool("Spinning", false);
            animator.SetBool("Shooting", false);

            if (rightWeapon == null)
            {
                rightWeapon = unarmedWeapon;
            }

            if (leftWeapon == null)
            {
                leftWeapon = unarmedWeapon;
            }

            CancelInvoke(nameof(Chase));
            switch (currentState)
            {
                case AttackState.chase:
                    InvokeRepeating(nameof(Chase), 0f, 0.1f);
                    agent.stoppingDistance = 0f;
                    break;
                case AttackState.search:
                    StartCoroutine(SearchRoutine());
                    agent.stoppingDistance = 0f;
                    break;
                case AttackState.attack:
                    Attack();
                    break;
            }
        }
    }

    protected virtual void Attack()
    {
        attacking = true;
        switch (rightWeapon.weaponType)
        {
            case EnemyWeaponStats.Weapons.sword:
                agent.stoppingDistance = rightWeapon.meleeRange;
                SwordAttack();
                break;
            case EnemyWeaponStats.Weapons.axe:
                agent.stoppingDistance = rightWeapon.meleeRange;
                AxeAttack();
                break;
            case EnemyWeaponStats.Weapons.crossbow:
                agent.stoppingDistance = rightWeapon.rangedRange;
                CrossbowAttack();
                break;
            case EnemyWeaponStats.Weapons.staff:
                agent.stoppingDistance = rightWeapon.rangedRange;
                StaffAttack();
                break;
            case EnemyWeaponStats.Weapons.unarmed:
                agent.stoppingDistance = rightWeapon.meleeRange;
                UnarmedAttack();
                break; 
        }

        Invoke(nameof(ResetAttack), rightWeapon.cooldown);
    }

    protected virtual void SwordAttack()
    {
        switch (leftWeapon.weaponType)
        {
            case EnemyWeaponStats.Weapons.shield:
                animator.SetTrigger("1HandMeleeAttack");
                animator.SetTrigger("ShieldBlock");
                animator.SetBool("Blocking", true);
                break;
            case EnemyWeaponStats.Weapons.sword:
                animator.SetTrigger("DualMeleeAttack");
                break;
            case EnemyWeaponStats.Weapons.axe:
                animator.SetTrigger("DualMeleeAttack");
                break;
            default:    // unarmed or missing reference
                animator.SetTrigger("2HandMeleeAttack");
                break;
        }
    }

    protected virtual void AxeAttack()
    {
        switch (leftWeapon.weaponType)
        {
            case EnemyWeaponStats.Weapons.shield:
                animator.SetTrigger("1HandMeleeAttack");
                animator.SetTrigger("ShieldBlock");
                animator.SetBool("Blocking", true);
                break;
            case EnemyWeaponStats.Weapons.sword:
                animator.SetTrigger("DualMeleeAttack");
                break;
            case EnemyWeaponStats.Weapons.axe:
                animator.SetTrigger("DualMeleeAttack");
                break;
            default:    // unarmed or missing reference
                animator.SetTrigger("MeleeSpin");
                animator.SetBool("Spinning", true);
                InvokeRepeating(nameof(SpinMove), 0f, 0.25f);
                break;
        }
    }

    protected virtual void CrossbowAttack()
    {
        if (IsPlayerInRange(rightWeapon.meleeRange))
        {
            switch (leftWeapon.weaponType)
            {
                case EnemyWeaponStats.Weapons.shield:
                    animator.SetTrigger("UnarmedAttack");
                    animator.SetTrigger("ShieldBlock");
                    animator.SetBool("Blocking", true);
                    break;
                default:    // unarmed or missing reference
                    animator.SetTrigger("UnarmedAttack");
                    break;
            }

        }
        else 
        { 
            switch (leftWeapon.weaponType)
            {
                case EnemyWeaponStats.Weapons.shield:
                    Invoke(nameof(ShootCrossbow), 0.3f);
                    animator.SetTrigger("1HandCrossbowAttack");
                    animator.SetTrigger("ShieldBlock");
                    animator.SetBool("Blocking", true);
                    break;
                default:    // unarmed or missing reference
                    Invoke(nameof(ShootCrossbow), 0.3f);
                    animator.SetTrigger("2HandCrossbowAttack");
                    break;
            }
        }
    }

    protected virtual void ShootCrossbow()
    {
        if (currentState != AttackState.attack) return;
        // Instantiate the bolt at the crossbow's position
        Transform arrowTransform = Instantiate(arrowPrefab, arrowSpawnTransform.position, Quaternion.identity);

        // Calculate the direction towards the player
        Vector3 direction = (player.position + new Vector3(0, 1, 0) - arrowSpawnTransform.position).normalized;

        // Rotate the arrow to face the player
        arrowTransform.rotation = Quaternion.LookRotation(direction);

        // Apply a forward force to the bolt
        arrowTransform.GetComponent<Rigidbody>().AddForce(direction * rightWeapon.projectileSpeed, ForceMode.Impulse);
    }

    protected virtual void StaffAttack()
    {
        if (IsPlayerInRange(rightWeapon.meleeRange))
        {
            switch (leftWeapon.weaponType)
            {
                case EnemyWeaponStats.Weapons.shield:
                    animator.SetTrigger("1HandMeleeAttack");
                    animator.SetTrigger("ShieldBlock");
                    animator.SetBool("Blocking", true);
                    break;
                default:    // unarmed or missing reference
                    animator.SetTrigger("2HandMeleeAttack");
                    break;
            }

        }
        else if (summonTimer >= 10f * rightWeapon.cooldown)
        {
            summonTimer = 0f;

            switch (leftWeapon.weaponType)
            {
                case EnemyWeaponStats.Weapons.shield:
                    animator.SetTrigger("Summon");
                    animator.SetTrigger("ShieldBlock");
                    animator.SetBool("Blocking", true);
                    break;
                default:    // unarmed or missing reference
                    animator.SetTrigger("Summon");
                    break;
            }
        }
        else
        {
            switch (leftWeapon.weaponType)
            {
                case EnemyWeaponStats.Weapons.shield:
                    Invoke(nameof(ShootMagic), 0.3f);
                    animator.SetTrigger("1HandStaffAttack");
                    animator.SetTrigger("ShieldBlock");
                    animator.SetBool("Blocking", true);
                    break;
                default:    // unarmed or missing reference
                    Invoke(nameof(ShootMagic), 0.3f);
                    animator.SetTrigger("1HandStaffAttack");
                    break;
            }
        }
    }

    protected virtual void ShootMagic()
    {
        if (currentState != AttackState.attack) return;
        Transform magicTransform = Instantiate(magicPrefab, magicSpawnTransform.position, Quaternion.identity);
        magicTransform.GetComponent<EnemyMagic>().SetPlayerTransform(player);
    }

    protected virtual void UnarmedAttack()
    {
        switch (leftWeapon.weaponType) 
        {
            case EnemyWeaponStats.Weapons.shield:
                animator.SetTrigger("ShieldBashAttack");
                animator.SetTrigger("ShieldBlock");
                animator.SetBool("Blocking", true);
                break;
            default:    // unarmed or missing reference
                animator.SetTrigger("UnarmedAttack");
                break;
        }
    }

    protected virtual void ResetAttack()
    {
        attacking = false;

        if (IsPlayerInRange(rightWeapon.meleeRange) || IsPlayerInRange(rightWeapon.rangedRange))
        {
            Attack();
        }
    }
    protected virtual void FacePlayer()
    {
        // Face the player
        if (IsPlayerInRange(rightWeapon.meleeRange) || IsPlayerInRange(rightWeapon.rangedRange) || aiMain.CanSeePlayer())
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            lastKnownPlayerPosition = player.position;
        }
    }

    protected virtual void Chase()
    {
        if (currentState != AttackState.chase) return;

        agent.speed = chaseSpeed;
        lastKnownPlayerPosition = player.position;
        agent.SetDestination(player.position);
    }

    protected virtual void SpinMove()
    {
        if (currentState != AttackState.attack) return;

        agent.speed = chaseSpeed / 2f;
        lastKnownPlayerPosition = player.position;
        agent.SetDestination(player.position);
    }

    protected virtual IEnumerator SearchRoutine()
    {
        searchTime = 0f;
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
            if (agent.remainingDistance < 0.1f)
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

    public virtual bool IsPlayerInRange(float range)
    {
        return Vector3.SqrMagnitude(transform.position - player.position) <= Mathf.Pow(range, 2);
    }

    public virtual void SetLastKnownPlayerPosition(Vector3 position)
    {
        lastKnownPlayerPosition = position;
        searchTime = 0f;
    }

    public virtual void DisableColliders()
    {
        //rightWeaponTransform.GetComponent<Collider>().enabled = false;
        //leftWeaponTransform.GetComponent<Collider>().enabled = false;
    }
}