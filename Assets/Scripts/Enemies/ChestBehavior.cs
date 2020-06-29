using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChestBehavior : MonoBehaviour, IMessageReceiver
{
    private NavMeshAgent agent;
    private Animator anim;

    [SerializeField] private PlayerScanner scanner;
    private GameObject target;

    private MeleeWeapon weapon;
    [SerializeField] private float attackRange;
    [SerializeField] private float losePlayerDistance;
    [SerializeField] private float attackCD;
    private float lastAttacked;
    private bool hasAttacked;

    [SerializeField] private List<GameObject> patrolPoints = new List<GameObject>();

    public EnemyState state = EnemyState.Idle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        weapon = GetComponentInChildren<MeleeWeapon>();
        weapon.wielder = this.gameObject;

        agent.stoppingDistance = attackRange;
    }

    private void Update()
    {
        anim.SetBool("Idle", state == EnemyState.Idle);
        anim.SetBool("Moving", state == EnemyState.Moving || state == EnemyState.Patrol);
        agent.isStopped = anim.GetCurrentAnimatorStateInfo(0).IsTag("BlockInput") ? true : false;
    }

    private void FixedUpdate()
    {
        DecideDecision();
    }

    private void DecideDecision()
    {
        if (state == EnemyState.Dead)
            return;

        switch (state)
        {
            case EnemyState.Idle:

                target = FindTarget();

                if (target != null)
                {
                    anim.SetTrigger("Scare");
                    transform.rotation = Quaternion.LookRotation(target.transform.position, Vector3.up);
                    state = EnemyState.Moving;
                }                

                break;

            case EnemyState.Moving:

                if (FindTarget() == null)
                {
                    target = null;
                    state = EnemyState.Idle;
                    break;
                }

                float distance = Vector3.Distance(transform.position, target.transform.position);
                distance = Mathf.Abs(distance);

                agent.SetDestination(target.transform.position);

                if (agent.remainingDistance <= attackRange)
                {
                    anim.SetTrigger("Attack");
                    state = EnemyState.Attack;
                    break;
                }
                

                break;

            case EnemyState.Attack:

                if (hasAttacked)
                {
                    lastAttacked += Time.deltaTime;

                    if (lastAttacked >= attackCD)
                    {
                        state = FindTarget() ? EnemyState.Moving : EnemyState.Idle;
                        lastAttacked = 0;
                        hasAttacked = false;
                    }
                }

                anim.SetBool("AttackCD", !hasAttacked);

                break;
        }

        if (target != null && !agent.isStopped)
            agent.SetDestination(target.transform.position);
    }

    public void OnReceiveMessage(MessageType type, object sender, object msg)
    {
        switch (type)
        {
            case MessageType.Damaged:

                Damaged((DamageAble.DamageData)msg);

                break;

            case MessageType.Dead:

                Death((DamageAble.DamageData)msg);

                break;
        }
    }

    private void Damaged(DamageAble.DamageData info)
    {
        anim.SetTrigger("Hit");
        state = EnemyState.Hit;

        target = info.damageOwner;
    }



    private void Death(DamageAble.DamageData data)
    {
        anim.SetTrigger("Death");
    }

    private GameObject FindTarget()
    {
        PlayerController player = scanner.detect(this.transform);

        return player?.gameObject;
    }

    private GameObject NextPatrol()
    {
        int rand = Random.Range(0, patrolPoints.Count);
        return patrolPoints[rand];
    }

    #region animatorEvents
    public void BeginAttack()
    {
        weapon.BeginAttack();
    }

    public void EndAttack()
    {
        weapon.EndAttack();
        target = null;
    }

    public void ChangeState(EnemyState newState)
    {
        state = newState;
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        scanner.EditorGizmo(transform);
    }
#endif
}

