using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class DamageZone : MonoBehaviour
{
    [SerializeField]private int damageAmount = 1;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    private void OnTriggerStay(Collider other)
    {
        var d = other.GetComponent<DamageAble>();
        if (d == null)
            return;

        var data = new DamageAble.DamageData()
        {
            damageAmount = this.damageAmount,
            damager = this,
            direction = Vector3.up,
        };

        d.ApplyDamage(data);
    }
}
