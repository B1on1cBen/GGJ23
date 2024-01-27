using UnityEngine;

public class NPC : MonoBehaviour
{
    static Transform player;

    public float moveSpeed;
    public bool canMove;
    Vector3 moveInput;

    Rigidbody rb;

    void Awake()
    {
        if (!player)
            player = GameObject.Find("Player").transform;

        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        moveInput = (player.position - transform.position).normalized;
        moveInput.y = 0;
    }

    void FixedUpdate()
    {
        if (canMove)
            Move();
    }

    private void Move()
    {
        rb.MovePosition(transform.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        transform.localScale = new Vector3(Mathf.Sign(moveInput.x),1,1);
    }
}
