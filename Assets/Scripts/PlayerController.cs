﻿using System.Collections;
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
    private bool canAttack;

    private float desiredForwardSpeed;
    private float forwardSpeed;
    private float verticalSpeed;

    const float k_GroundAcceleration = 20f;
    const float k_GroundDeceleration = 25f;

    private Vector2 MoveDirection;

    private bool IsMoving
    {
        get
        {
            return !Mathf.Approximately(MoveDirection.sqrMagnitude, 0);
        }
    }
    private bool IsAttacking
    {
        get
        {
            return true;
        }
    }

    private float idleTimer = 0f;

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

        anim.SetBool("Grounded", cc.isGrounded);
    }

    private void FixedUpdate()
    {
        anim.SetFloat("StateTime", Mathf.Repeat(anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));

        CalculateForwardMovement();

        Move();

        TimeOutToIdle();
    }

    private float CalculateForwardMovement()
    {
        if (MoveDirection.sqrMagnitude > 1f)
            MoveDirection.Normalize();

        desiredForwardSpeed = MoveDirection.magnitude * maxForwardSpeed;

        float acceleration = IsMoving ? k_GroundAcceleration : k_GroundDeceleration;

        forwardSpeed = Mathf.MoveTowards(forwardSpeed, desiredForwardSpeed, acceleration * Time.deltaTime);

        anim.SetFloat("ForwardSpeed", forwardSpeed);

        return forwardSpeed;
    }

    private void Move()
    {
        Vector3 movement = new Vector3(MoveDirection.x, 0, MoveDirection.y);

        cc.Move(movement * forwardSpeed * Time.deltaTime);
    }

    private void OnAnimatorMove()
    {
        Vector3 movement;

        movement = anim.deltaPosition;

        movement += forwardSpeed * transform.forward * Time.deltaTime;

        Debug.Log("move");

        cc.Move(movement);
    }

    private void TimeOutToIdle()
    {
        bool inputDetected = IsMoving || Input.GetMouseButtonDown(0);

        Debug.Log(inputDetected);

        if (!inputDetected)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleTimeout)
            {
                idleTimer = 0f;
                anim.SetTrigger("Idle");
                Debug.Log("Going to Idle State");
            }
        }
        else
        {
            idleTimer = 0f;
            anim.ResetTrigger("Idle");
        }

        //m_Animator.SetBool(m_HashInputDetected, inputDetected);
    }
}
