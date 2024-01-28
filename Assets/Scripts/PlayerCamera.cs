using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public bool trackingPlayer;
    public Vector3 offset;
    public Transform evilDude;
    [HideInInspector] public Vector3 npcOffset;
    public float smoothingSpeed;
    Transform player;
    public bool tracking;

    Vector3 wantedPosition;
    public CameraShake shake;

    void Awake()
    {
        player = GameObject.Find("Player").transform;
        wantedPosition = evilDude.position - Vector3.up + offset;
        shake = gameObject.GetComponent<CameraShake>();
        shake.enabled = false;
    }

    void FixedUpdate()
    {
        if (trackingPlayer)
            wantedPosition = player.position - Vector3.up + Vector3.right + offset + npcOffset;

        if (tracking)
            transform.position = Vector3.Lerp(transform.position, wantedPosition, smoothingSpeed * Time.fixedDeltaTime);   
    }

    public void PanToEvilDude()
    {
        wantedPosition = player.position - Vector3.up + offset;
    }
}
