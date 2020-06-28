using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RatGoblin : MonoBehaviour, IMessageReceiver
{
    private NavMeshAgent agent;
    private Animator anim;
    private DamageAble damageAble;

    private MeleeWeapon weapon;

    private GameObject target;

    private EnemyState state;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        damageAble = GetComponent<DamageAble>();

        weapon = GetComponentInChildren<MeleeWeapon>();

        state = EnemyState.Idle;
    }

    private void Update()
    {
        switch (state)
        {
            case EnemyState.Idle:



                break;
        }
    }


    public void OnReceiveMessage(MessageType type, object sender, object msg)
    {

    }
}

public enum EnemyState { Idle, Patrol, Running, Attack, Death}
