using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [SerializeField]ParticleSystem effect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<DamageAble>(out DamageAble d))
        {
            effect.Play();
            d.Heal(5);
            Destroy(gameObject, 0.3f);
        }
    }
}
