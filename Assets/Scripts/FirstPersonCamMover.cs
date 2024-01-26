using UnityEngine;

[RequireComponent(typeof(Rigidbody))]   // Makes sure this object has a rigidbody. If one is not found, it is automatically created. Thanks Unity!
[AddComponentMenu("_Medly/Movement/First Person Cam Mover")]

public class FirstPersonCamMover : AbstractPlayerMover {

    [Header("Specialized Movement")]
    [SerializeField] protected float gravity = 10;
    [SerializeField] protected float maxVelocityChange = 10;

    [Header("Jumping")]
    [SerializeField] protected float jumpForce = 10;
    protected bool onGround = true;

    [Header("Animation")]
    [SerializeField] protected Animator HeadAnimator;

    protected int speedParam;
    protected Transform cam;
    protected Rigidbody rigidBody;
    
    protected virtual void Awake() {
        cam = Camera.main.transform;

        // An error check to make sure that the player has a rigidbody.
        // Unity has default error messages, but this one will stop Awake() from asking for a rigidbody that isn't there.
        if (gameObject.GetComponent<Rigidbody>() == null) {
            Debug.LogError("Hey, your player does not have a rigidbody!");
            return;
        }

        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
        rigidBody.useGravity = false; // We will be adding in gravity manually to allow for greater control.

        speedParam  = Animator.StringToHash("speedParam");
    }

    // Gets input from keyboard or controller. (Overridden to get default values instead of raw)
    protected override void RetrieveInput() {
        H = Input.GetAxis("Horizontal");
        V = Input.GetAxis("Vertical");
    }

    protected override void FixedUpdate() {
        RetrieveInput();

        // Rotate (all the time)
        Rotate();

        PerformMovement();
        Jump();
        AddGravity(); // Always add gravity force to object.

        // A check that allows the logic for jumping to function.
        onGround = false;
    }

    protected void PerformMovement() {
        if (Mathf.Abs(V) > gamepadError || Mathf.Abs(H) > gamepadError) {
            Movement();
        }
        else if (HeadAnimator && HeadAnimator.GetFloat(speedParam) != 0){
            HeadAnimator.SetFloat(speedParam, 0);
        }
    }

    protected void Jump() {
        if(Input.GetButton("Jump") && onGround) {
            Vector3 velocity = rigidBody.velocity;
	        rigidBody.velocity = new Vector3(velocity.x, JumpVelocity, velocity.z);
        }
    }
   
    protected void AddGravity() {
        rigidBody.AddForce(0, -gravity * rigidBody.mass, 0);
    }

    // Moves the object, according to the direction it faces and input.
    protected override void Movement() 
    {
        // Get direction of movement.
        Vector3 input = new Vector3(H, 0, V); // Might have to make this not RAW - DONE!
        Vector3 velocity = rigidBody.velocity;

        // Adjust the length of input vectors so that moving diagonal does not make you faster.
        if (input.sqrMagnitude > 1)
            input.Normalize();

        if (HeadAnimator)
        {
            if(onGround)
                HeadAnimator?.SetFloat(speedParam, input.magnitude);
            else 
                HeadAnimator?.SetFloat(speedParam, 0);
        }

        Vector3 movement = (input * moveSpeed);
        movement = transform.TransformDirection(movement);

        Vector3 velocityChange = movement - velocity;
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;
        rigidBody.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    // Rotates the object based on the rotation of the camera.
    protected override void Rotate() 
    {
        Quaternion tempRot = cam.rotation; // Get the rotation we want from the camera.

        tempRot.x = 0; // Make sure the player's rotation stays locked on the X-Z plane.
        tempRot.z = 0;

        // Rotate the player with final rotation values.
        if (smoothRotation) {
            //Smoothly rotate our player to face the direction it moves to drastically improve aesthetics.
            rigidBody.rotation = Quaternion.Lerp(rigidBody.rotation, tempRot, rotationSmooth * Time.deltaTime);
            transform.rotation = rigidBody.rotation;
        }
        else {
            rigidBody.rotation = tempRot;
            transform.rotation = rigidBody.rotation;
        }
    }   
    
    // Tells us how fast to fall.
    float JumpVelocity { get { return Mathf.Sqrt(2 * jumpForce * gravity); }}
}
