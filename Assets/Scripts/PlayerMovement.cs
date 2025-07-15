using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // Variables to determine movement
    public Camera playerCamera;
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
    }
}
