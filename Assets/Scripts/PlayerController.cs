using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController cc;
    private Animator anim;

    [SerializeField] private float maxForwardSpeed = 8f;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField] private float minTurnSpeed = 400f;
    [SerializeField] private float maxTurnSpeed = 1200f;
    [SerializeField] private float idleTimeout = 5f;
    [SerializeField] private bool  canAttack;

    private Vector2 MoveDirection;

    private bool IsMoving
    {
        get
        {
            return Mathf.Approximately(MoveDirection.magnitude, 0);
        }
    }
    private bool IsAttacking
    {
        get
        {
            return true;
        }
    }

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        MoveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("attacking");
            anim.SetTrigger("Attack");
        }
    }

    private void FixedUpdate()
    {
        anim.SetFloat("StateTime", Mathf.Repeat(anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));
    }

    private void CalculateForwardMovement()
    {
        if (MoveDirection.sqrMagnitude > 1f)
            MoveDirection.Normalize();
    }
}
