using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rat_Behavior : MonoBehaviour, IMessageReceiver
{
    private NavMeshAgent agent;
    private Animator anim;

    [SerializeField] private PlayerScanner scanner;
    private GameObject target;

    private MeleeWeapon weapon;
    [SerializeField] private float attackRange;
    [SerializeField] private float losePlayerDistance;

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
        anim.SetBool("Patrol", state == EnemyState.Patrol);
        anim.SetBool("Combat", state == EnemyState.Moving);
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

                if (target == null)
                {
                    target = NextPatrol();
                    state = EnemyState.Patrol;
                }

                state = EnemyState.Moving;

                break;

            case EnemyState.Patrol:

                if (FindTarget() != null)
                {
                    target = FindTarget();
                    agent.SetDestination(target.transform.position);
                    state = EnemyState.Moving;
                    break;
                }

                else if (agent.remainingDistance <= 2f)
                {
                    target = NextPatrol();
                    agent.SetDestination(target.transform.position);
                }                

                break;

            case EnemyState.Moving:

                float distance = Vector3.Distance(transform.position, target.transform.position);
                distance = Mathf.Abs(distance);

                if ( agent.remainingDistance <= attackRange)
                {
                    anim.SetTrigger("Attack");
                    state = EnemyState.Attack;
                    break;
                }

                else if (FindTarget() == null)
                {
                    state = EnemyState.Patrol;
                    break;
                }

                break;

            case EnemyState.Attack:

                

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
        state = EnemyState.Patrol;
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

public enum EnemyState { Idle, Patrol, Moving, Hit, Attack, Dead }
