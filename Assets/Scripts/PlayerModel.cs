using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    GameManager gameManager;
    void Awake()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
    }

    public void SlapHit()
    {
        gameManager.SlapHitStun();
    }
}
