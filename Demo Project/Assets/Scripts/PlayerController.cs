using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;

    //Movement Settings

    [SerializeField] private float speed;
    [SerializeField] private float gravity;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float turnSmoothVelocity; //variables to smooth turning the character
    [SerializeField] private float turnSmoothTime;

    [SerializeField] private float jumpHeight;
    [SerializeField] private float doubleJumpMultiplier;
    private bool doubleJump = false;

    //Acceleration settings
    [SerializeField] private float maxSpeed;
    [SerializeField] private float timeZeroToMax;
    [SerializeField] private float accelRatePerSec; //difference in velocity per sec
    [SerializeField] private float forwardVelocity; //

    private float directionY; //temp value for direction

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    void Start()
    {
        speed = 6.0f;
        gravity = 19.81f;
        turnSmoothTime = 0.1f;
        jumpHeight = 14.0f;
        doubleJumpMultiplier = 1.0f;
        maxSpeed = 10.0f;
        timeZeroToMax = 5.8f;

        accelRatePerSec = maxSpeed / timeZeroToMax; //equation of acceleration
        forwardVelocity = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); //sphere at bottom of the player to check for collisions for gravity checks

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");


        Vector3 direction = new Vector3(horizontal, 0.0f, vertical).normalized; //normalize to not double the speed when pressing 2 or more keys

        direction = JumpCheck(direction);

        //velocity.y -= gravity + Time.deltaTime;

        SetGravity();

        controller.Move(velocity * Time.deltaTime); // only Y axis for jump functionality

        Walk(direction);

        if (controller.isGrounded) //&& velocity.y < 0
        {
            velocity.y = -2f;
        }

    }

    private void Walk(Vector3 direction)
    {
        if (direction.magnitude >= 0.1) //get input to move
        {


            // Atan2 -> returns angle between x-axis and vector starting at 0 to x,y
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y; //face direction the player is moving
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime); //function to smooth the angle turn
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * (Vector3.forward); //face direction based on camera

            JumpCheck(moveDir);


            forwardVelocity += accelRatePerSec;
            speed += forwardVelocity;
            speed = Mathf.Min(forwardVelocity, maxSpeed);


            controller.Move(moveDir.normalized * speed * Time.deltaTime);


        }
    }

    private void SetGravity()
    {
        directionY -= gravity * Time.deltaTime;
        velocity.y = directionY;
    }

    private Vector3 JumpCheck(Vector3 direction)
    {
        if (controller.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                doubleJump = true;
                //velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity); //better check to force player on the ground - eh no
                directionY = jumpHeight;
            }
            if (Input.GetButtonDown("Jump") && direction.magnitude >= 0.1)
            {
                doubleJump = true;
                directionY = jumpHeight;
            }
        }
        else
        {
            if (Input.GetButtonDown("Jump") && doubleJump)
            {
                directionY = jumpHeight * doubleJumpMultiplier;
                doubleJump = false;
            }
        }

        return direction;
    }
}
