using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class DamageAble : MonoBehaviour
{
    public int maxHP = 5;
    public int currentHP;

    public float invulnerbilityTime;
    public bool isInvulnerable;
    private float timeSinceLastHit = 0.0f;

    [Range(0.0f, 360.0f)]
    public float hitAngle = 360.0f;

    [Range(0.0f, 360.0f)]
    public float hitForwardRotation = 360.0f;

    public UnityEvent onRecieveDamage, onDeath, onHitFailed, onInvulnerableEnd, onHeal;

    public List<MonoBehaviour> messageReceivers; 

    public struct DamageData
    {
        public MonoBehaviour damager;
        public GameObject damageOwner;
        public int damageAmount;
        public Vector3 direction;
        public Vector3 damageSource;
    }

    private void Start()
    {
        currentHP = maxHP;

        foreach(MonoBehaviour mono in GetComponentsInParent<IMessageReceiver>())
        {
            messageReceivers.Add(mono);
        }
    }

    public void ApplyDamage(DamageData data)
    {
        if (currentHP < 0)
            return;

        if (isInvulnerable)
            return;

        Debug.Log("This ran how many times?");

        Vector3 forward = transform.forward;
        forward = Quaternion.AngleAxis(hitForwardRotation, transform.up) * forward;

        Vector3 posFromDamager = data.damageSource - transform.position;
        posFromDamager -= transform.up * Vector3.Dot(transform.up, posFromDamager);

        if (Vector3.Angle(forward, posFromDamager) > hitAngle * 0.5f)
        {
            onHitFailed.Invoke();
            return;
        }
            

        isInvulnerable = true;
        currentHP -= data.damageAmount;

        if (currentHP <= 0)
            onDeath.Invoke();

        else onRecieveDamage.Invoke();

        var messageType = currentHP <= 0 ? MessageType.Dead : MessageType.Damaged;

        for (var i = 0; i < messageReceivers.Count; ++i)
        {
            var receiver = messageReceivers[i] as IMessageReceiver;
            receiver.OnReceiveMessage(messageType, this, data);
        }
    }

    public void Heal(int amount)
    {
        currentHP += amount;

        if (currentHP >= maxHP)
            currentHP = maxHP;

        onHeal.Invoke();
    }

    private void Update()
    {
        if (isInvulnerable)
        {
            timeSinceLastHit += Time.deltaTime;

            if (timeSinceLastHit > invulnerbilityTime)
            {
                timeSinceLastHit = 0.0f;
                isInvulnerable = false;
                onInvulnerableEnd.Invoke();
            }
        }
    }

    public void UpdateHitAngle(float angle, float forward)
    {
        hitAngle = angle;
        hitForwardRotation = forward;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 forward = transform.forward;
        forward = Quaternion.AngleAxis(hitForwardRotation, transform.up) * forward;

        if (Event.current.type == EventType.Repaint)
        {
            UnityEditor.Handles.color = Color.blue;
            UnityEditor.Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(forward), 1.0f,
                EventType.Repaint);
        }


        UnityEditor.Handles.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        forward = Quaternion.AngleAxis(-hitAngle * 0.5f, transform.up) * forward;
        UnityEditor.Handles.DrawSolidArc(transform.position, transform.up, forward, hitAngle, 1.0f);
    }
#endif
}

