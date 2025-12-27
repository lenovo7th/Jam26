using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class TimerUICreator : EditorWindow
{
    [MenuItem("Tools/Create Timer UI")]
    public static void CreateTimerUI()
    {
        // Canvas oluştur
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // CanvasScaler ayarları
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // ========== TIMER TEXT (Sol Üst) ==========
        GameObject timerObj = new GameObject("TimerText");
        timerObj.transform.SetParent(canvasObj.transform, false);
        
        TextMeshProUGUI timerText = timerObj.AddComponent<TextMeshProUGUI>();
        timerText.text = "60";
        timerText.fontSize = 72;
        timerText.fontStyle = FontStyles.Bold;
        timerText.alignment = TextAlignmentOptions.TopLeft;
        timerText.color = Color.white;

        RectTransform timerRect = timerObj.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0, 1);
        timerRect.anchorMax = new Vector2(0, 1);
        timerRect.pivot = new Vector2(0, 1);
        timerRect.anchoredPosition = new Vector2(30, -30);
        timerRect.sizeDelta = new Vector2(200, 100);

        // ========== GAME OVER PANEL ==========
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform, false);
        
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Game Over Text
        GameObject gameOverTextObj = new GameObject("GameOverText");
        gameOverTextObj.transform.SetParent(gameOverPanel.transform, false);
        
        TextMeshProUGUI gameOverText = gameOverTextObj.AddComponent<TextMeshProUGUI>();
        gameOverText.text = "GAME OVER";
        gameOverText.fontSize = 120;
        gameOverText.fontStyle = FontStyles.Bold;
        gameOverText.alignment = TextAlignmentOptions.Center;
        gameOverText.color = Color.red;

        RectTransform goTextRect = gameOverTextObj.GetComponent<RectTransform>();
        goTextRect.anchorMin = new Vector2(0.5f, 0.6f);
        goTextRect.anchorMax = new Vector2(0.5f, 0.6f);
        goTextRect.sizeDelta = new Vector2(800, 150);

        // Restart Button
        GameObject restartBtn = CreateButton(gameOverPanel.transform, "RestartButton", "TEKRAR OYNA", new Vector2(0, -50));
        
        // Quit Button
        GameObject quitBtn = CreateButton(gameOverPanel.transform, "QuitButton", "ÇIKIŞ", new Vector2(0, -130));

        // Panel'i başlangıçta gizle
        gameOverPanel.SetActive(false);

        // ========== GAME MANAGER ==========
        GameObject gameManager = new GameObject("GameManager");
        GameTimer timer = gameManager.AddComponent<GameTimer>();
        GameOverUI gameOverUI = gameManager.AddComponent<GameOverUI>();

        // Serialized field'ları ayarla
        SerializedObject timerSO = new SerializedObject(timer);
        timerSO.FindProperty("timerText").objectReferenceValue = timerText;
        timerSO.ApplyModifiedProperties();

        SerializedObject gameOverSO = new SerializedObject(gameOverUI);
        gameOverSO.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        gameOverSO.FindProperty("gameOverText").objectReferenceValue = gameOverText;
        gameOverSO.FindProperty("restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        gameOverSO.FindProperty("quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
        gameOverSO.FindProperty("gameTimer").objectReferenceValue = timer;
        gameOverSO.ApplyModifiedProperties();

        // EventSystem kontrolü
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Debug.Log("Timer UI oluşturuldu!");
        EditorUtility.DisplayDialog("Başarılı!", "Timer UI ve Game Over ekranı oluşturuldu!", "Tamam");
        
        Selection.activeGameObject = canvasObj;
    }

    private static GameObject CreateButton(Transform parent, string name, string text, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);


        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f);
        button.colors = colors;

        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = new Vector2(300, 60);

        // Button Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = text;
        btnText.fontSize = 32;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return buttonObj;
    }
}
