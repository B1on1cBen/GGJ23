using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed;
    public bool canMove;
    public bool cutscene;
    public float moveThreshold = 0.02f;
    [SerializeField] public Vector3 moveInput;

    public Animator animator;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (canMove)
            moveInput = new Vector3(
                Input.GetAxis("Horizontal"),
                0,
                Input.GetAxis("Vertical")
            );
    }

    void FixedUpdate()
    {
        if (cutscene)
            return;
            
        if (canMove && moveInput.magnitude > moveThreshold){
            Move();
            animator.SetBool("Moving", true);
        }
        else
        {
            animator.SetBool("Moving", false);
        }
    }

    private void Move()
    {
        rb.MovePosition(transform.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        transform.localScale = new Vector3(Mathf.Sign(moveInput.x),1,1);
    }
}
