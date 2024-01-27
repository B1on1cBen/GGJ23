using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Intro,
        Game,
        InDialogue,
        Lose,
        Slap
    }

    public static List<NPC> npcs = new List<NPC>();

    public float launchForce;
    public float totalTime;
    public float chargeAmountPerPress;
    public float chargeDecay;
    public float totalCharge;
    public float chargeBarSmoothing;
    public float cameraZoomSmoothing;
    public float cameraZoomMax;
    float cameraZoomMin;
    public AudioClip[] slapSounds;

    [Space]
    [SerializeField] Slider slider;
    [SerializeField] Image chargeBar;
    [SerializeField] LayerMask npcTouchPlayerMask;
    [SerializeField] Player player;
    [SerializeField] GameObject dialogueUI;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] new PlayerCamera camera;

    public GameState state;
    AudioSource audioSource;
    float currentTime;
    float currentCharge;
    NPC currentNPC;
    int currentDialogueIndex;
    float lastPressTime;
    float wantedCameraZoom;

    List<GameObject> npcsToDestroy = new List<GameObject>();

    void Awake()
    {
        currentTime = totalTime;
        audioSource = GetComponent<AudioSource>();
        dialogueUI.SetActive(false);
        cameraZoomMin = Camera.main.orthographicSize;

        state = GameState.Game;
        wantedCameraZoom = cameraZoomMin;
    }

    void Update()
    {
        UpdateCameraZoom();

        if (state == GameState.Intro)
        {
            // Stop movement
            player.canMove = false;
            foreach(NPC npc in npcs)
                npc.canMove = false;
            
            // Do something, but nothing for now

        }
        else if (state == GameState.Game)
        {
            UpdateTimer();
            CheckForNPCTouches();
        }
        else if (state == GameState.InDialogue)
        {
            UpdateChargeInput();
            UpdateChargeBar();
            UpdateDialogue();
        }
    }

    private void UpdateDialogue()
    {
        if (!currentNPC)
            return;

        if (audioSource.isPlaying)
            return;

        currentDialogueIndex++;
        if (currentDialogueIndex > currentNPC.dialogue.text.Length - 1)
        {
            currentNPC.Leave();
            currentNPC = null;
            EndDialogue();
            return;
        }
        else
        {        
            audioSource.PlayOneShot(currentNPC.dialogue.voice[currentDialogueIndex]);
            text.text = currentNPC.dialogue.text[currentDialogueIndex];
        }
    }

    private void UpdateCameraZoom()
    {
        Camera.main.orthographicSize = Mathf.Lerp(
            Camera.main.orthographicSize, 
            wantedCameraZoom,
            cameraZoomSmoothing * Time.deltaTime);
    }

    private bool CanPress => Time.time >= lastPressTime;

    private void UpdateChargeInput()
    {
        if (Input.GetButtonDown("Jump") && !currentNPC.launched && CanPress)
        {
            lastPressTime = Time.time + currentNPC.timeBetweenPresses;
            currentCharge += chargeAmountPerPress;
            if (currentCharge >= totalCharge)
            {
                chargeBar.fillAmount = 1;
                Slap();
            }
        }

        currentCharge -= chargeDecay * Time.deltaTime;
        currentCharge = Mathf.Clamp(currentCharge, 0, totalCharge);
    }

    private void UpdateChargeBar()
    {
        chargeBar.fillAmount = Mathf.Lerp(chargeBar.fillAmount, currentCharge / totalCharge, chargeBarSmoothing * Time.deltaTime);
        wantedCameraZoom = Mathf.Lerp(cameraZoomMin, cameraZoomMax, chargeBar.fillAmount);
    }

    private void Slap()
    {
        state = GameState.Slap;
        Debug.Log("SLAP!!!");

        // Murder NPC after slap, then go back to normal
        currentNPC.Launch(launchForce);
        npcsToDestroy.Add(currentNPC.gameObject);
        currentNPC = null;
        Invoke("DestroyNPC", 3);
        
        EndDialogue();
    }

    public void PlaySlapSound()
    {
        audioSource.PlayOneShot(slapSounds[Random.Range(0, slapSounds.Length)]);
    }

    private void DestroyNPC()
    {
        foreach(var npc in npcsToDestroy)
            Destroy(npc);
    }

    private void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            currentTime = 0;
            slider.value = currentTime / totalTime;
            state = GameState.Lose;
            Lose();
        }

        slider.value = currentTime / totalTime;
    }

    private void CheckForNPCTouches()
    {
        var colliders = Physics.OverlapSphere(player.transform.position, 2, npcTouchPlayerMask);
        if (colliders.Length > 0)
        {
            state = GameState.InDialogue;
            PlayDialogue(colliders[0].GetComponent<NPC>());
        }
    }

    public void PlayDialogue(NPC talkingNPC)
    {
        currentCharge = 0;
        currentDialogueIndex = 0;
        chargeBar.fillAmount = 0;
        currentNPC = talkingNPC;
        totalCharge = currentNPC.totalPresses;

        // Stop movement
        player.canMove = false;
        foreach(NPC npc in npcs)
            npc.canMove = false;

        // Set up dialogue
        dialogueUI.SetActive(true);

        audioSource.PlayOneShot(currentNPC.dialogue.voice[0]);
        text.text = currentNPC.dialogue.text[0];

        // Move camera
        camera.npcOffset = new Vector3(
            (currentNPC.transform.position.x - player.transform.position.x) * 0.5f - camera.offset.x,
            0, 
            2);
    }

    private void EndDialogue()
    {
        audioSource.Stop();
        text.text = "";
        dialogueUI.SetActive(false);
        camera.npcOffset = Vector3.zero;
        wantedCameraZoom = cameraZoomMin;

        // Start movement
        player.canMove = true;
        foreach(NPC npc in npcs)
            npc.canMove = true;

        state = GameState.Game;
    }

    private void Lose()
    {
        SceneManager.LoadScene(2);
    }
}