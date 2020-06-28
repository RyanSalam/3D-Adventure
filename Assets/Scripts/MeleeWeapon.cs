using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    public int damage = 1;

    [System.Serializable]
    public class AttackPoint
    {
        public float radius;
        public Vector3 offset;
        public Transform attackRoot;

#if UNITY_EDITOR
        [System.NonSerialized] public List<Vector3> previousPoints = new List<Vector3>();
#endif
    }


    public ParticleSystem hit_effect;
    public LayerMask targetLayer;

    public AttackPoint[] attackPoints = new AttackPoint[0];
    public GameObject wielder;

    private Vector3[] previousPoints = null;
    private Vector3 direction;

    public bool inAttack; // Turn this later to private

    private static RaycastHit[] RaycastCache = new RaycastHit[32];
    private static Collider[] colliderCache = new Collider[32];

    private void Awake()
    {
        if (hit_effect != null)
        {
            hit_effect.Stop();
        }
    }

    public void BeginAttack()
    {
        inAttack = true;

        previousPoints = new Vector3[attackPoints.Length];

        for (int i = 0; i < attackPoints.Length; ++i)
        {
            Vector3 worldPos = attackPoints[i].attackRoot.position +
                                attackPoints[i].attackRoot.TransformVector(attackPoints[i].offset);

            previousPoints[i] = worldPos;

#if UNITY_EDITOR
            attackPoints[i].previousPoints.Clear();
            attackPoints[i].previousPoints.Add(previousPoints[i]);
#endif
        }
    }
  

    private void FixedUpdate()
    {
        if (inAttack)
        {
            for (int i = 0; i < attackPoints.Length; ++i)
            {
                AttackPoint point = attackPoints[i];

                Vector3 worldPos = point.attackRoot.position + point.attackRoot.TransformVector(point.offset);
                Vector3 attackVector = worldPos - previousPoints[i];

                if (attackVector.magnitude < 0.001f)
                {
                    attackVector = Vector3.forward * 0.0001f;
                }

                Ray ray = new Ray(worldPos, attackVector.normalized);

                int contacts = Physics.SphereCastNonAlloc(ray, point.radius, RaycastCache, attackVector.magnitude, ~0, QueryTriggerInteraction.Ignore); 

                for (int k = 0; k < contacts; ++k)
                {
                    Collider col = RaycastCache[k].collider;

                    if (col != null)
                        CheckDamage(col, point);
                }

                previousPoints[i] = worldPos;

#if UNITY_EDITOR
                point.previousPoints.Add(previousPoints[i]);
#endif
            }
        }
    }

    public void EndAttack()
    {
        inAttack = false;

#if UNITY_EDITOR
        for (int i = 0; i < attackPoints.Length; ++i)
        {
            attackPoints[i].previousPoints.Clear();
        }
#endif
    }

    private bool CheckDamage(Collider target, AttackPoint point)
    {
        DamageAble victim = target.GetComponent<DamageAble>();

        if (victim == null)
            return false;

        if (victim.gameObject == wielder)
            return true;

        DamageAble.DamageData data = new DamageAble.DamageData
        {
            damageAmount = damage,
            damager = this,
            direction = direction,
            damageSource = wielder.transform.position
        };

        victim.ApplyDamage(data);


        if (hit_effect != null)
        {
            hit_effect.transform.position = point.attackRoot.position;
            hit_effect.time = 0;
            hit_effect.Play();
        }

        return true;
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < attackPoints.Length; ++i)
        {
            AttackPoint pts = attackPoints[i];

            if (pts.attackRoot != null)
            {
                Vector3 worldPos = pts.attackRoot.TransformVector(pts.offset);
                Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.4f);
                Gizmos.DrawSphere(pts.attackRoot.position + worldPos, pts.radius);
            }

            if (pts.previousPoints.Count > 1)
            {
                UnityEditor.Handles.DrawAAPolyLine(10, pts.previousPoints.ToArray());
            }
        }
    }

#endif

}
