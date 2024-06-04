using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class AIAttack : NetworkBehaviour
{
    protected float summonTimer = 0f;
    protected float searchTimer;
    protected NetworkVariable<bool> attacking = new NetworkVariable<bool>(false);

    protected AIMain aiMain;
    protected Animator animator;
    protected NavMeshAgent agent;
    protected EnemyStats enemyStats;

    [SerializeField]
    protected Vector3 targetPlayerPosition;
    [SerializeField]
    protected NetworkVariable<Vector3> lastKnownPlayerPosition = new NetworkVariable<Vector3>(default);

    [SerializeField] protected EnemyWeaponStats rightWeapon;
    [SerializeField] protected EnemyWeaponStats leftWeapon;

    [SerializeField] protected Transform arrowSpawnTransform;
    [SerializeField] protected Transform magicSpawnTransform;

    private Coroutine chaseRoutine;
    private Coroutine searchRoutine;
    private Coroutine spinMoveRoutine;
    private Coroutine raiseMinionRoutine;

    [SerializeField] protected NetworkVariable<AttackState> currentState;
    protected enum AttackState
    {
        none,
        chase,
        attack,
        search
    }

    protected void Awake()
    {
        aiMain = GetComponent<AIMain>();
        animator = aiMain.GetAnimator();
        agent = aiMain.GetAgent();
        enemyStats = aiMain.GetEnemyStats();

        currentState.OnValueChanged += (previous, current) => StateChangeHandler();
    }

    protected virtual void OnEnable()
    {
        agent.enabled = true;
        animator.SetBool("Combat", true);

        if (targetPlayerPosition != Vector3.zero)
        {
            SetLastKnownPlayerPositionServerRpc(targetPlayerPosition);
        }

        if (rightWeapon == null) rightWeapon = enemyStats.unarmedWeapon;
        if (leftWeapon == null) leftWeapon = enemyStats.unarmedWeapon;
    }

    protected virtual void OnDisable()
    {
        aiMain.ResetTargetPlayerClientRpc();
        animator.SetBool("Combat", false);
        SetCurrentStateServerRpc(AttackState.none);
    }

    protected virtual void Update()
    {
        if (!IsOwner) return;
        StateHandler();

        summonTimer += Time.deltaTime;
        if (aiMain.GetTargetPlayer() != null)
        {
            targetPlayerPosition = aiMain.GetTargetPlayer().position;
        }
        else
        {
            targetPlayerPosition = Vector3.zero;
        }
    }

    protected virtual void StateHandler()
    {
        if (aiMain.CanSeePlayer() && (IsPlayerInRange(rightWeapon.rangedRange) || IsPlayerInRange(rightWeapon.meleeRange)) || attacking.Value)
        {
            SetCurrentStateServerRpc(AttackState.attack);
            FacePlayer();
        }
        else if (aiMain.CanSeePlayer() || aiMain.IsPlayerInAlertRange())
        {
            SetCurrentStateServerRpc(AttackState.chase);
        }
        else
        {
            SetCurrentStateServerRpc(AttackState.search);
        }
    }

    protected virtual void StateChangeHandler()
    {
        animator.SetBool("Blocking", false);
        animator.SetBool("Spinning", false);
        animator.SetBool("Shooting", false);

        if (chaseRoutine != null)
        {
            StopCoroutine(chaseRoutine);
            chaseRoutine = null;
        }

        if (searchRoutine != null)
        {
            StopCoroutine(searchRoutine);
            searchRoutine = null;
        }

        if (spinMoveRoutine != null)
        {
            StopCoroutine(spinMoveRoutine);
            spinMoveRoutine = null;
        }

        if (raiseMinionRoutine != null)
        {
            StopCoroutine(raiseMinionRoutine);
            raiseMinionRoutine = null;
        }

        if (rightWeapon == null) rightWeapon = enemyStats.unarmedWeapon;
        if (leftWeapon == null) leftWeapon = enemyStats.unarmedWeapon;

        switch (currentState.Value)
        {
            case AttackState.none:
                break;
            case AttackState.chase:
                chaseRoutine = StartCoroutine(ChaseRoutine());
                break;
            case AttackState.attack:
                Attack();
                break;
            case AttackState.search:
                searchRoutine = StartCoroutine(SearchRoutine());
                break;
        }
    }

    protected virtual void Attack()
    {
        SetAttackingBoolServerRpc(true);
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

    protected virtual void ResetAttack()
    {
        if (aiMain.CanSeePlayer() && (IsPlayerInRange(rightWeapon.meleeRange) || IsPlayerInRange(rightWeapon.rangedRange)))
        {
            Attack();
            return;
        }
        else
        {
            SetAttackingBoolServerRpc(false);
        }
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
                spinMoveRoutine = StartCoroutine(SpinMoveRoutine());
                break;
        }
    }

    protected virtual IEnumerator SpinMoveRoutine()
    {
        agent.speed = enemyStats.chaseSpeed / 2f;
        agent.stoppingDistance = 1.5f;

        if (IsOwner)
        {
            print("Spinning");
            aiMain.SetAgentDestinationServerRpc(targetPlayerPosition);
        } 
        
        yield return null;
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
        if (currentState.Value != AttackState.attack) return;

        Transform arrowTransform = Instantiate(enemyStats.arrowPrefab, arrowSpawnTransform.position, Quaternion.identity);
        Vector3 direction = (targetPlayerPosition + new Vector3(0, 1, 0) - arrowSpawnTransform.position).normalized;

        arrowTransform.rotation = Quaternion.LookRotation(direction);
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
        else if (summonTimer >= 8 * rightWeapon.cooldown)
        {
            summonTimer = 0f;

            switch (leftWeapon.weaponType)
            {
                case EnemyWeaponStats.Weapons.shield:
                    SpawnMinionServerRpc();
                    animator.SetTrigger("Summon");
                    animator.SetTrigger("ShieldBlock");
                    animator.SetBool("Blocking", true);
                    break;
                default:    // unarmed or missing reference
                    SpawnMinionServerRpc();
                    animator.SetTrigger("Summon");
                    break;
            }
        }
        else
        {
            switch (leftWeapon.weaponType)
            {
                case EnemyWeaponStats.Weapons.shield:
                    Invoke(nameof(SpawnMagicServerRpc), 0.3f);
                    animator.SetTrigger("1HandStaffAttack");
                    animator.SetTrigger("ShieldBlock");
                    animator.SetBool("Blocking", true);
                    break;
                default:    // unarmed or missing reference
                    Invoke(nameof(SpawnMagicServerRpc), 0.3f);
                    animator.SetTrigger("1HandStaffAttack");
                    break;
            }
        }
    }

    protected virtual IEnumerator RaiseMinionRoutine(Transform minionTransform)
    {
        AIMain minionAIMain = minionTransform.GetComponent<AIMain>();
        minionAIMain.SetStunnedServerRpc(true, 3.5f);
        minionAIMain.SetAttackingServerRpc(true);
        minionAIMain.GetAgent().enabled = false;
        minionAIMain.GetAnimator().SetTrigger("SummonMinion");
        minionTransform.GetComponent<AIAttack>().SetLastKnownPlayerPositionServerRpc(targetPlayerPosition);

        float timer = 0f;
        Vector3 startPosition = minionTransform.position;
        Vector3 endPosition = minionTransform.position + (transform.up * 2f);
        while (true)
        {
            timer += Time.deltaTime;
            minionTransform.position = Vector3.Lerp(startPosition, endPosition, timer / 0.2f);
            if (timer >= 0.2f) break;
            yield return null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SpawnMinionServerRpc()
    {
        Transform minionTransform = Instantiate(enemyStats.minionPrefab,
            transform.position + (transform.forward * 2f) - (transform.up * 2f), transform.rotation);
        minionTransform.GetComponent<NetworkObject>().Spawn();

        raiseMinionRoutine = StartCoroutine(RaiseMinionRoutine(minionTransform));
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SpawnMagicServerRpc()
    {
        if (currentState.Value != AttackState.attack) return;
        Transform magicTransform = Instantiate(enemyStats.magicPrefab, magicSpawnTransform.position, transform.rotation);
        magicTransform.GetComponent<NetworkObject>().Spawn();
        magicTransform.GetComponent<EnemyMagic>().SetPlayerTransform(aiMain.GetTargetPlayer());
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

    protected virtual IEnumerator ChaseRoutine()
    {
        agent.speed = enemyStats.chaseSpeed;
        agent.stoppingDistance = 0.1f;
        while (true)
        {
            if (IsOwner)
            {
                aiMain.SetAgentDestinationServerRpc(targetPlayerPosition);
            }

            yield return new WaitForSeconds(0.05f);
            if (currentState.Value != AttackState.chase)
            {
                SetLastKnownPlayerPositionServerRpc(targetPlayerPosition);
                break;
            }
        }
    }

    protected virtual IEnumerator SearchRoutine()
    {
        searchTimer = 0f;
        agent.speed = enemyStats.searchSpeed;
        agent.stoppingDistance = 0.1f;

        while (searchTimer < enemyStats.searchDuration)
        {
            if (currentState.Value != AttackState.search)
            {
                yield break;
            }

            if (agent.isActiveAndEnabled && agent.remainingDistance < 0.5f)
            {
                Vector3 randomDirection = Random.insideUnitSphere * enemyStats.searchRadius;
                randomDirection += lastKnownPlayerPosition.Value;
                NavMeshHit hit;
                NavMesh.SamplePosition(randomDirection, out hit, enemyStats.searchRadius, 1);

                if (IsOwner)
                {
                    aiMain.SetAgentDestinationServerRpc(hit.position);

                    aiMain.ResetTargetPlayerIdServerRpc();
                    aiMain.ResetTargetPlayerClientRpc();
                }
            }

            searchTimer += Time.deltaTime;
            yield return null;
        }

        aiMain.SetAttackingServerRpc(false);
    }

    protected virtual void FacePlayer()
    {
        Vector3 direction = (targetPlayerPosition - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * enemyStats.rotationSpeed);
        SetLastKnownPlayerPositionServerRpc(targetPlayerPosition);
    }

    protected virtual bool IsPlayerInRange(float range)
    {
        return Vector3.SqrMagnitude(transform.position - targetPlayerPosition) <= Mathf.Pow(range, 2);
    }

    public virtual void SetSearchTimer(float newTimer)
    {
        searchTimer = newTimer;
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SetCurrentStateServerRpc(AttackState newState)
    {
        currentState.Value = newState;
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SetLastKnownPlayerPositionServerRpc(Vector3 newPosition)
    {
        lastKnownPlayerPosition.Value = newPosition;
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SetAttackingBoolServerRpc(bool value)
    {
        attacking.Value = value;
    }
}
