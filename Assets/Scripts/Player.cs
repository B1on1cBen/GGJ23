using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed;
    public bool canMove;
    public float moveThreshold = 0.02f;
    Vector3 moveInput;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        moveInput = new Vector3(
            Input.GetAxis("Horizontal"),
            0,
            Input.GetAxis("Vertical")
        );
    }

    void FixedUpdate()
    {
        if (canMove && moveInput.magnitude > moveThreshold)
            Move();
    }

    private void Move()
    {
        rb.MovePosition(transform.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        transform.localScale = new Vector3(Mathf.Sign(moveInput.x),1,1);
    }
}
