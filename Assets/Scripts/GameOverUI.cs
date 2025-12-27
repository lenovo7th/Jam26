using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("References")]
    [SerializeField] private GameTimer gameTimer;

    void Awake()
    {
        // Game Over panelini başlangıçta gizle
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Timer'ı bul
        if (gameTimer == null)
            gameTimer = FindFirstObjectByType<GameTimer>();

        // Timer event'ine abone ol
        if (gameTimer != null)
            gameTimer.OnTimeUp += ShowGameOver;

        // Buton event'lerini bağla
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    void OnDestroy()
    {
        if (gameTimer != null)
            gameTimer.OnTimeUp -= ShowGameOver;
    }

    public void ShowGameOver()
    {
        Debug.Log("Game Over gösteriliyor!");
        
        // Oyunu durdur
        Time.timeScale = 0f;
        
        // Game Over panelini göster
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Cursor'ı göster (eğer gizliyse)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        // Zamanı normale döndür
        Time.timeScale = 1f;
        
        // Sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        Time.timeScale = 1f;
    }
}
