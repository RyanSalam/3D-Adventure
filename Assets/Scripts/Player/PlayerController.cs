using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IMessageReceiver
{
    public static PlayerController instance;

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

    private bool isAttacking;
    public MeleeWeapon weapon;
    private DamageAble damageAble;

    private float desiredForwardSpeed;
    private float forwardSpeed;
    private float verticalSpeed;

    const float k_GroundAcceleration = 20f;
    const float k_GroundDeceleration = 25f;
    const float k_JumpAbortSpeed = 10f;
    const float k_StickingGravityProportion = 0.3f;

    private Vector2 MoveDirection;

    AnimatorStateInfo stateInfo;
    private int BlockingLayer;


    private bool CanMove
    {
        get
        {
            return ! (anim.GetCurrentAnimatorStateInfo(0).IsTag("BlockInput") || anim.GetCurrentAnimatorStateInfo(0).IsTag("BlockMovement"));
        }
    }

    private bool CanJump
    {
        get
        {
            return  cc.isGrounded && CanMove;
        }
    }

    private bool CanAttack
    {
        get
        {
            return !anim.GetCurrentAnimatorStateInfo(0).IsTag("BlockInput");
        }
    }
    private bool IsMoving
    {
        get
        {
            return !Mathf.Approximately(MoveDirection.sqrMagnitude, 0);
        }
    }

    private float idleTimer = 0f;

    void Awake()
    {
        if (instance == null)
            instance = this;

        else
            Destroy(gameObject);

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        cameraTransform = Camera.main.transform;

        weapon = GetComponentInChildren<MeleeWeapon>();
        weapon.wielder = gameObject;

        damageAble = GetComponentInChildren<DamageAble>();
        damageAble.messageReceivers.Add(this);

        BlockingLayer = anim.GetLayerIndex("Blocking");
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

        CalculateVerticalMovement();

        UpdateOrientation();

        UpdateBlock();

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
        if (cc.isGrounded)
        {
            verticalSpeed = -gravity * k_StickingGravityProportion;

            if (Input.GetButtonDown("Jump") && CanJump)
            {
                anim.SetTrigger("Jump");
                verticalSpeed = jumpSpeed;
            }
        }

        else
        {
            if (!Input.GetButton("Jump") && verticalSpeed > 0.0f)
            {
                verticalSpeed -= k_JumpAbortSpeed * Time.fixedDeltaTime;
                
            }

            if (Mathf.Approximately(verticalSpeed, 0f))
                verticalSpeed = 0f;

            verticalSpeed -= gravity * Time.deltaTime; 
        }

        anim.SetFloat("VerticalSpeed", verticalSpeed);
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
        
        // Note to future self don't remove these Again

        forward.y = 0;
        right.y = 0;

        Vector3 desiredMoveDirection = (right * MoveDirection.x + forward * MoveDirection.y).normalized;
        desiredMoveDirection.y = 0;

        //desiredMoveDirection += verticalSpeed * Vector3.up * Time.deltaTime; 
      
        cc.Move(desiredMoveDirection * forwardSpeed * Time.fixedDeltaTime);
        cc.Move(verticalSpeed * Vector3.up * Time.fixedDeltaTime);
    }

    public void OnReceiveMessage(MessageType type, object sender, object msg)
    {
        switch (type)
        {
            case MessageType.Damaged:

                DamageAble.DamageData damageData = (DamageAble.DamageData)msg;
                Damaged(damageData);

                break;
        }
    }

    private void Damaged(DamageAble.DamageData data)
    {
        anim.SetTrigger("Hit");

        Vector3 forward = data.damageSource - transform.position;
        forward.y = 0;

        Vector3 localHurt = transform.InverseTransformDirection(forward);

        anim.SetFloat("HurtFromZ", localHurt.z);
    }

    public void BeginAttack()
    {
        isAttacking = true;
        weapon.BeginAttack();
    }

    public void EndAttack()
    {
        isAttacking = false;
        weapon.EndAttack();
    }

    private void UpdateBlock()
    {
        anim.SetLayerWeight(BlockingLayer, Input.GetAxis("Fire2"));
        anim.SetBool("Blocking", anim.GetLayerWeight(BlockingLayer) > 0);

        damageAble.hitAngle = anim.GetBool("Blocking") ? 180f : 360f;
        damageAble.hitForwardRotation = anim.GetBool("Blocking") ? 180f : 360f;
    }

    public void BlockAttack()
    {
        StartCoroutine(Blocked());
    }

    private IEnumerator Blocked()
    {
        BeginAttack();

        yield return new WaitForSeconds(1f);

        EndAttack();
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
