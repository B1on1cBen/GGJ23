using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
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
        Slap,
        Anger,
        FirstSlap
    }

    public static List<NPC> npcs = new List<NPC>();

    public bool anger;
    public bool inIntro = true;
    public float launchForce;
    public float totalTime;
    public float chargeAmountPerPress;
    public float chargeDecay;
    public float totalCharge;
    public float chargeBarSmoothing;
    public float cameraZoomSmoothing;
    public float cameraZoomMax;
    public bool canSlap;
    float cameraZoomMin;
    public AudioClip[] slapSounds;
    public AudioSource chargeAudioSource;
    [SerializeField] AudioSource NPCAudioSource;
    [SerializeField] AudioSource SlapAudioSource;
    public AudioClip chargeSound;
    public float chargePitchMin;
    public float chargePitchMax;
    public float chargeReverbMin;
    public float chargeReverbMax;
    public float muffleTime;
    public float lowPassMin = 100;
    public float lowPassMax = 22000;
    public NPC evilIntroNPC;
    public NPC HeroIntro1NPC;
    public NPC HeroIntro2NPC;
    public NPC Sage;
    public GameObject mash;

    [Space]
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider slider;
    [SerializeField] Image chargeBar;
    [SerializeField] LayerMask npcTouchPlayerMask;
    [SerializeField] Player player;
    [SerializeField] GameObject dialogueUI;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] new PlayerCamera camera;
    [SerializeField] Animator playerAnimator;

    public GameState state;
    float currentTime;
    float currentCharge;
    NPC currentNPC;
    int currentDialogueIndex;
    float lastPressTime;
    float wantedCameraZoom;
    bool charging;

    float muffle;
    float angerStartTime;
    private float angerEndTime;
    List<GameObject> npcsToDestroy = new List<GameObject>();

    void Awake()
    {
        //state = GameState.Game;
        currentTime = totalTime;
        dialogueUI.SetActive(false);
        cameraZoomMin = Camera.main.orthographicSize;
        wantedCameraZoom = cameraZoomMin;

        // Stop movement
        player.canMove = false;
        foreach(NPC npc in npcs)
            npc.canMove = false;
    }

    public void PlayEvilIntro()
    {
        PlayDialogue(evilIntroNPC);
    }

    public void PlayHeroIntro1()
    {
        PlayDialogue(HeroIntro1NPC);
    }

    public void PlayHeroIntro2()
    {
        PlayDialogue(HeroIntro2NPC);
    }

    void Update()
    {
        UpdateCameraZoom();

        if (state == GameState.Intro)
        {
            CheckForNPCTouches();
        }
        else if (state == GameState.Game)
        {
            UpdateTimer();
            CheckForNPCTouches();
        }
        else if (state == GameState.InDialogue)
        {
            UpdateChargeBar();
            UpdateChargeInput();
            UpdateDialogue();
        }
        else if (state == GameState.Anger)
        {
            if (!anger)
                UpdateDialogue();
            else
            {
                muffle = angerStartTime / angerEndTime;
                muffle = Mathf.Clamp01(muffle);
                float lowpass = Mathf.Lerp(lowPassMax, lowPassMin, muffle);
                audioMixer.SetFloat("Lowpass", lowpass);
            }
        }
        else if (state == GameState.FirstSlap)
        {
            if (Input.GetButtonDown("Jump"))
                mash.SetActive(false);

            UpdateChargeBar();
            UpdateChargeInput();
        }
    }

    public void EndAnger()
    {
        NPCAudioSource.Stop();
        text.text = "I DON'T HAVE TIME FOR THIS!";
    }

    private void UpdateDialogue()
    {
        if (!currentNPC)
            return;

        if (NPCAudioSource.isPlaying)
            return;

        currentDialogueIndex++;
        if (currentDialogueIndex > currentNPC.dialogue.text.Length - 1)
        {
            currentNPC.Leave();
            EndDialogue();
            return;
        }
        else
        {        
            NPCAudioSource.PlayOneShot(currentNPC.dialogue.voice[currentDialogueIndex]);
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
        if (!Input.GetButtonDown("Jump"))
            return;

        if (!charging)
        {
            charging = true;
            playerAnimator.SetTrigger("Charge");
        }

        audioMixer.SetFloat("ChargeReverb", Mathf.Lerp(chargeReverbMin, chargeReverbMax, chargeBar.fillAmount));
        chargeAudioSource.pitch = Mathf.Lerp(chargePitchMin, chargePitchMax, chargeBar.fillAmount);
        chargeAudioSource.PlayOneShot(chargeSound);
        
        playerAnimator.SetFloat("ChargeTime", chargeBar.fillAmount);

        if (!currentNPC.launched && CanPress)
        {
            lastPressTime = Time.time + currentNPC.timeBetweenPresses;
            currentCharge += chargeAmountPerPress;
            if (currentCharge >= totalCharge)
            {
                chargeBar.fillAmount = 1;
                wantedCameraZoom = cameraZoomMax;
                audioMixer.SetFloat("ChargeReverb", chargePitchMax);
                chargeAudioSource.pitch = chargePitchMax;
                chargeAudioSource.PlayOneShot(chargeSound);
                playerAnimator.SetFloat("ChargeTime", chargeBar.fillAmount);
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

    public void ExitIntro()
    {
        state = GameState.FirstSlap;
        canSlap = true;
        mash.SetActive(true);
    }

    public void StartGame()
    {
        state = GameState.Game;
        player.canMove = true;
        foreach(NPC npc in npcs)
            npc.canMove = true;
        camera.trackingPlayer = true;
    }

    public void PlaySlapSound()
    {
        SlapAudioSource.PlayOneShot(slapSounds[Random.Range(0, slapSounds.Length)]);
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
            if (inIntro)
                SageDialogue(colliders[0].GetComponent<NPC>());
            else
                PlayDialogue(colliders[0].GetComponent<NPC>());
        }
    }

    private void SageDialogue(NPC sage)
    {
        currentNPC = sage;
        currentDialogueIndex = 0;
        sage.canMove = false;
        dialogueUI.SetActive(true);

        NPCAudioSource.PlayOneShot(currentNPC.dialogue.voice[0]);
        text.text = currentNPC.dialogue.text[0];

        camera.npcOffset = new Vector3(
            (currentNPC.transform.position.x - player.transform.position.x) * 0.5f - camera.offset.x,
            0, 
            0);

        state = GameState.Anger;
    }

    public void PlayDialogue(NPC talkingNPC)
    {
        state = GameState.InDialogue;
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

        NPCAudioSource.PlayOneShot(currentNPC.dialogue.voice[0]);
        text.text = currentNPC.dialogue.text[0];

        // Move camera
        camera.npcOffset = new Vector3(
            (currentNPC.transform.position.x - player.transform.position.x) * 0.5f - camera.offset.x,
            0, 
            0);
    }

    private void EndDialogue()
    {
        NPCAudioSource.Stop();
        text.text = "";
        dialogueUI.SetActive(false);

        if (currentNPC.ghost)
            return;

        camera.npcOffset = Vector3.zero;
        wantedCameraZoom = cameraZoomMin;

        // Start movement
        player.canMove = true;
        foreach(NPC npc in npcs)
            npc.canMove = true;

        state = GameState.Game;
        currentNPC = null;
    }

    private void Lose()
    {
        SceneManager.LoadScene(2);
    }

    public void ActivateSage()
    {
        state = GameState.Intro;
        Sage.canMove = true;
    }

    public void StartAnger()
    {
        anger = true;
        angerStartTime = Time.time;
        angerEndTime = Time.time + muffleTime;
    }
}