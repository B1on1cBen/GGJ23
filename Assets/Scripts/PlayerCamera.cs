using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Vector3 offset;
    [HideInInspector] public Vector3 npcOffset;
    public float smoothingSpeed;
    Transform player;

    void Awake()
    {
        player = GameObject.Find("Player").transform;    
    }

    void FixedUpdate()
    {
        Vector3 wantedPosition = player.position - Vector3.up + offset + npcOffset;
        transform.position = Vector3.Lerp(transform.position, wantedPosition, smoothingSpeed * Time.fixedDeltaTime);   
    }
}
