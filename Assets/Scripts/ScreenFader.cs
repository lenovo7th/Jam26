using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Ekranı smooth şekilde siyaha çevirir ve geri açar.
/// KULLANIM:
/// 1. Canvas oluştur (Screen Space - Overlay)
/// 2. Canvas'a Panel ekle, siyah yap, alpha=0
/// 3. Bu scripti Canvas'a ekle ve Panel'i FadePanel'e ata
/// 4. Koddan: ScreenFader.Instance.FadeToBlack(() => { ... });
/// </summary>
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Header("Ayarlar")]
    public Image fadePanel;
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // FadePanel yoksa otomatik oluştur
        if (fadePanel == null)
        {
            CreateFadePanel();
        }

        // Başlangıçta görünmez yap
        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 0f;
            fadePanel.color = c;
            fadePanel.raycastTarget = false;
        }
    }

    void CreateFadePanel()
    {
        // Canvas kontrolü
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // En üstte
        }

        // CanvasScaler ekle
        if (GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        // GraphicRaycaster ekle
        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        // Panel oluştur
        GameObject panelObj = new GameObject("FadePanel");
        panelObj.transform.SetParent(transform, false);

        fadePanel = panelObj.AddComponent<Image>();
        fadePanel.color = new Color(0, 0, 0, 0);
        fadePanel.raycastTarget = false;

        // Tam ekran yap
        RectTransform rt = fadePanel.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Ekranı siyaha çevirir, bitince callback çağırır
    /// </summary>
    public void FadeToBlack(System.Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(0f, 1f, onComplete));
    }

    /// <summary>
    /// Ekranı siyahtan normale döndürür, bitince callback çağırır
    /// </summary>
    public void FadeFromBlack(System.Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(1f, 0f, onComplete));
    }

    /// <summary>
    /// Tam geçiş: siyaha git -> action çalıştır -> geri aç
    /// </summary>
    public void DoFadeTransition(System.Action duringBlack, System.Action onComplete = null)
    {
        FadeToBlack(() =>
        {
            duringBlack?.Invoke();
            FadeFromBlack(onComplete);
        });
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, System.Action onComplete)
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        Color color = fadePanel.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadePanel.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadePanel.color = color;

        onComplete?.Invoke();
    }
}
