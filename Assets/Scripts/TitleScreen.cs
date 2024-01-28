using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    AudioSource chargeAudioSource;
    [SerializeField] private GameObject title;
    [SerializeField] AudioSource slapAudioSource;
    [SerializeField] private float chargePitchMin;
    [SerializeField] Image chargeBar;
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private int chargeAmountPerPress;
    [SerializeField] private int totalCharge;
    [SerializeField] private float chargePitchMax;
    [SerializeField] private float chargeBarSmoothing;
    [SerializeField] private AudioClip slapSound;
    [SerializeField] GameObject prompt;
    private float lastPressTime;
    private int currentCharge;

    public bool Credits;

    bool started;

    void Awake()
    {
        if (Credits)
            return;

        chargeAudioSource = GetComponent<AudioSource>();
    }

    public void Play() => SceneManager.LoadScene(1);
    public void Quit() => Application.Quit();

    private void Update()
    {
        if (Credits)
            return;

        chargeBar.fillAmount = Mathf.Lerp(chargeBar.fillAmount, currentCharge / totalCharge, chargeBarSmoothing * Time.deltaTime);
        UpdateCharge();
    }

    private bool CanPress => Time.time >= lastPressTime;

    private void UpdateCharge()
    {        
        if (!started)
        {
            started = true;
            //prompt.SetActive(false);
        }

        if (CanPress && Input.GetButtonDown("Jump"))
        {
            Slap();
            // lastPressTime = Time.time + .25f;
            // currentCharge += chargeAmountPerPress;    
            // currentCharge = Mathf.Clamp(currentCharge, 0, totalCharge);
            // chargeAudioSource.PlayOneShot(chargeSound);
            return;
        }
        
        chargeAudioSource.pitch = Mathf.Lerp(chargePitchMin, chargePitchMin, chargeBar.fillAmount);

        if (currentCharge >= totalCharge)
        {
            chargeBar.fillAmount = 1;
            chargeAudioSource.pitch = chargePitchMax;
            //chargeAudioSource.PlayOneShot(chargeSound);
            Slap();
        }
    }

    private void Slap()
    {
        if (!title.activeInHierarchy)
            return;

        title.SetActive(false);
        prompt.SetActive(false);
        slapAudioSource.PlayOneShot(slapSound);
        Invoke("Play", 2);
    }
}
