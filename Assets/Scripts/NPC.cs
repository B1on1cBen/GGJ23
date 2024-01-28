using System;
using UnityEngine;

public class NPC : MonoBehaviour
{
    static Transform player;

    public bool ghost;
    public Dialogue dialogue;
    public float timeBetweenPresses;
    public float totalPresses;
    public float moveSpeed;
    public bool canMove;
    Vector3 moveInput;

    // Add a detect range

    Rigidbody rb;
    Animator animator;

    public bool launched;
    public CameraShake shake;

    void Awake()
    {
        if (!player)
            player = GameObject.Find("Player").transform;

        shake = gameObject.GetComponent<CameraShake>();

        if (shake)
            shake.enabled = false;
            
        GameManager.npcs.Add(this);
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (ghost)
            return;

        moveInput = (player.position - transform.position).normalized;
        moveInput.y = 0;
    }

    void FixedUpdate()
    {
        if (canMove && !ghost)
            Move();
    }

    private void Move()
    {
        rb.MovePosition(transform.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        transform.localScale = new Vector3(-Mathf.Sign(moveInput.x),1,1);
    }

    public void Launch(float force)
    {
        launched = true;
        rb.AddForce(new Vector3(Mathf.Sign(transform.localScale.x), 1, 0) * force, ForceMode.Impulse);
        animator.SetTrigger("spin");
    }

    internal void Leave()
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        GameManager.npcs.Remove(this);
    }
}
