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
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    private Transform cameraTransform;

    private bool canAttack;

    private float desiredForwardSpeed;
    private float forwardSpeed;
    private float verticalSpeed;

    const float k_GroundAcceleration = 20f;
    const float k_GroundDeceleration = 25f;

    private Vector2 MoveDirection;

    AnimatorStateInfo stateInfo;

    readonly int Hash_Attack1 = Animator.StringToHash("Attack 1");
    readonly int Hash_Attack2 = Animator.StringToHash("Attack 2");
    readonly int Hash_Attack3 = Animator.StringToHash("Attack 3");

    private bool CanMove
    {
        get
        {
            return !anim.GetCurrentAnimatorStateInfo(0).IsTag("Blockmovement");
        }
    }
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
            return anim.GetCurrentAnimatorStateInfo(0).shortNameHash == Hash_Attack1 ||
                anim.GetCurrentAnimatorStateInfo(0).shortNameHash == Hash_Attack2 ||
                anim.GetCurrentAnimatorStateInfo(0).shortNameHash == Hash_Attack3;
        }
    }

    private float idleTimer = 0f;

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        MoveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(anim.GetCurrentAnimatorStateInfo(0).IsTag("Blockmovement"));
            anim.SetTrigger("Attack");
        }

        anim.SetBool("Grounded", cc.isGrounded);
    }

    private void FixedUpdate()
    {
        anim.SetFloat("StateTime", Mathf.Repeat(anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));

        CalculateForwardMovement();

        Move();

        UpdateOrientation();

        TimeOutToIdle();
    }

    private void CalculateForwardMovement()
    {
        if (MoveDirection.sqrMagnitude > 1f)
            MoveDirection.Normalize();

        desiredForwardSpeed = MoveDirection.magnitude * maxForwardSpeed;

        float acceleration = IsMoving ? k_GroundAcceleration : k_GroundDeceleration;

        forwardSpeed = Mathf.Lerp(forwardSpeed, desiredForwardSpeed, acceleration * Time.fixedDeltaTime);

        anim.SetFloat("ForwardSpeed", forwardSpeed);
    }

    private void CalculateVerticalMovement()
    {

    } 

    private void UpdateOrientation()
    {
        Vector3 direction = new Vector3(MoveDirection.x, 0, MoveDirection.y).normalized;

        if (IsMoving)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0, angle, 0);
        }            
    }

    private void Move()
    {
        if (!CanMove)
            return;

        Vector3 inputDirection = new Vector3(MoveDirection.x, 0, MoveDirection.y);

        Vector3 forward = cameraTransform.forward.normalized;
        Vector3 right = cameraTransform.right.normalized;

        forward.y = 0;
        right.y = 0;

        Vector3 desiredMoveDirection = (right * MoveDirection.x + forward * MoveDirection.y).normalized;
        desiredMoveDirection.y = 0;
      
        cc.Move(desiredMoveDirection * forwardSpeed * Time.fixedDeltaTime);
    }

    private void TimeOutToIdle()
    {
        bool inputDetected = IsMoving || Input.GetMouseButtonDown(0);

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
