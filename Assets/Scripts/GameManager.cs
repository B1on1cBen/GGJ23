using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool startTimer;
    public float totalTime;
    public Slider slider;
    float currentTime;

    void Awake()
    {
        currentTime = totalTime;
    }

    void Update()
    {
        if (startTimer)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = 0;
                UpdateTimerBar();
                Lose();
            }
            UpdateTimerBar();
        }
    }

    private void UpdateTimerBar()
    {
        slider.value = currentTime / totalTime;
    }

    private void Lose()
    {
        SceneManager.LoadScene(2);
    }
}
