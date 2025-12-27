using System;
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float startTime = 60f;
    [SerializeField] private TextMeshProUGUI timerText;
    
    private float currentTime;
    private bool isRunning = true;

    public event Action OnTimeUp;
    public event Action<float> OnTimeChanged;

    public float CurrentTime => currentTime;
    public float StartTime => startTime;
    public bool IsRunning => isRunning;

    void Awake()
    {
        currentTime = startTime;
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(0, currentTime);
        
        UpdateTimerDisplay();
        OnTimeChanged?.Invoke(currentTime);

        if (currentTime <= 0)
        {
            TimeUp();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(currentTime);
            timerText.text = seconds.ToString();
            
            // Son 10 saniyede kırmızı yap
            if (currentTime <= 10f)
            {
                timerText.color = Color.red;
            }
        }
    }

    private void TimeUp()
    {
        isRunning = false;
        Debug.Log("Süre doldu! Game Over!");
        OnTimeUp?.Invoke();
    }

    // Süreye zaman ekle (ileride kullanılacak)
    public void AddTime(float amount)
    {
        if (!isRunning) return;
        
        currentTime += amount;
        Debug.Log($"+{amount} saniye eklendi! Toplam: {Mathf.CeilToInt(currentTime)}");
        
        // Rengi normale döndür
        if (timerText != null && currentTime > 10f)
        {
            timerText.color = Color.white;
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        if (currentTime > 0)
            isRunning = true;
    }

    public void ResetTimer()
    {
        currentTime = startTime;
        isRunning = true;
        if (timerText != null)
            timerText.color = Color.white;
        UpdateTimerDisplay();
    }
}
