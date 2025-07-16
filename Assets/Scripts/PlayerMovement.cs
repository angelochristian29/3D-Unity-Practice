using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // Variables to determine movement
    public Camera playerCamera;
    Animator animator;
    AudioSource audioSource;

    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    private float def_walk_speed, def_run_speed; // to change speed in editor

    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    // instantiate variables
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private bool canMove = true;

    void Start()
    {
        /* Gets component on character to hook up movement
           Locks cursor for FPS and hides cursor
        */
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        def_walk_speed = walkSpeed; //  change2
        def_run_speed = runSpeed; // change3
    }

    void Update()
    {
        // new vector for moving player forward and turning right
        Vector3 forward = transform.TransformDirection(Vector3.forward); // 0,0,1
        Vector3 right = transform.TransformDirection(Vector3.right); // 1,0,0

        /* 
            sets isRunning to T or F whether they press shift key
            set current speed x and y depending on if they are running
            set y direction to y component of movement vector
            update movement direction by multiplying 0,0,1 * current x speed then add right vector * current y speed
        */
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // if player presses space make y component of movement vector to jump height else keep it 0
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // make player fall by lowering movement vector by gravity * delta time
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // press key to crouch and change height and mov otherwise keep it default
        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;

        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = def_walk_speed;
            runSpeed = def_run_speed;
        }

        // move player with movement vector
        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            // restricting player camera movement
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            // rotating camera and player model in game
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            Attack();
        }
    }

    // ---------- //
    // ANIMATIONS //
    // ---------- //

    public const string IDLE = "Idle";
    public const string WALK = "Walk";
    public const string ATTACK1 = "Attack 1";
    public const string ATTACK2 = "Attack 2";

    string currentAnimationState;

    public void ChangeAnimationState(string newState)
    {
        // STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF
        if (currentAnimationState == newState) return;

        // Play animation
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }

    void SetAnimations()
    {
        // if not attacking
        if (!attacking)
        {
            if (moveDirection.x == 0 && moveDirection.z == 0)
            {
                ChangeAnimationState(IDLE);
            }
            else
            {
                ChangeAnimationState(WALK);
            }
        }
    }

    // ------------------- //
    // ATTACKING BEHAVIOUR //
    // ------------------- //

    [Header("Attacking")]
    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage = 1;
    public LayerMask attackLayer;

    public GameObject hitEffect;
    public AudioClip swordSwing;
    public AudioClip hitSound;

    bool attacking = false;
    bool readyToAttack = true;
    int attackCount;

    public void Attack()
    {
        // Debug.Log("Attacking");
        // return;

        if (!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        // reset variables afer attacking wait until attack speed time then send attack raycast and wait until attack delay
        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        audioSource.pitch = 1;
        audioSource.PlayOneShot(swordSwing);

        if (attackCount == 0)
        {
            // if you haven't attacked play attack anim
            ChangeAnimationState(ATTACK1);
            attackCount++;
        }
        else
        {
            ChangeAnimationState(ATTACK2);
            attackCount = 0;
        }
    }

    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
    }

    void AttackRaycast()
    {
        // cast ray from camera in forward direction, output raycasthit obj at attack distance and apply attack layer
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            HitTarget(hit.point);

            if (hit.transform.TryGetComponent<Actor>(out Actor T))
            {
                T.TakeDamage(attackDamage);
            }
        }
    }

    void HitTarget(Vector3 pos)
    {
        // when hitting something play hit sound
        audioSource.pitch = 1;
        audioSource.PlayOneShot(hitSound);

        // create knife hit mark and destroy after 20 seconds
        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
        Destroy(GO, 20);
    }
}
