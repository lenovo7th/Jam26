using UnityEngine;
using System.Collections;

/// <summary>
/// Asansör portal sistemi - E tuşu ile teleportasyon.
/// KULLANIM:
/// 1. Asansör objesine bu scripti ekle
/// 2. Box Collider ekle ve Is Trigger = true yap
/// 3. Inspector'dan Target Elevator'a diğer asansörü ata
/// 4. (Opsiyonel) Ses dosyalarını ata
/// </summary>
public class ElevatorPortal : MonoBehaviour
{
    [Header("Hedef Asansör")]
    [Tooltip("Player bu asansöre gidecek")]
    public ElevatorPortal targetElevator;

    [Header("Teleport Ayarları")]
    [Tooltip("Player'ın çıkış noktası (boşsa asansörün önü)")]
    public Transform exitPoint;
    [Tooltip("Teleport sonrası player'ın bakış yönü")]
    public bool flipDirection = true;

    [Header("Sesler")]
    public AudioClip doorCloseSound;
    public AudioClip doorOpenSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.8f;

    [Header("UI İpucu")]
    public string interactionText = "Asansörü kullanmak için [E] tuşuna bas";

    private bool playerInRange = false;
    private bool isTransitioning = false;
    private Transform playerTransform;
    private AudioSource audioSource;

    void Start()
    {
        // AudioSource ekle
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D ses

        // Exit point yoksa oluştur
        if (exitPoint == null)
        {
            GameObject exitObj = new GameObject("ExitPoint");
            exitObj.transform.SetParent(transform);
            exitObj.transform.localPosition = new Vector3(0, 0, 1.5f); // Asansörün önü
            exitPoint = exitObj.transform;
        }

        // Collider kontrolü
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(2f, 2.5f, 2f);
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"[ElevatorPortal] {gameObject.name} collider'ı trigger değil! Is Trigger'ı açın.");
        }
    }

    void Update()
    {
        if (playerInRange && !isTransitioning && Input.GetKeyDown(KeyCode.E))
        {
            if (targetElevator != null)
            {
                StartCoroutine(TeleportSequence());
            }
            else
            {
                Debug.LogWarning("[ElevatorPortal] Hedef asansör atanmamış!");
            }
        }
    }

    IEnumerator TeleportSequence()
    {
        isTransitioning = true;

        // ScreenFader kontrolü - yoksa oluştur
        if (ScreenFader.Instance == null)
        {
            GameObject faderObj = new GameObject("ScreenFader");
            faderObj.AddComponent<ScreenFader>();
            yield return null; // Bir frame bekle
        }

        // 1. Kapı kapanma sesi
        PlaySound(doorCloseSound);

        // 2. Fade to black
        bool fadeComplete = false;
        ScreenFader.Instance.FadeToBlack(() => fadeComplete = true);

        // Fade bitene kadar bekle
        while (!fadeComplete)
        {
            yield return null;
        }

        // 3. Player'ı teleport et
        if (playerTransform != null && targetElevator != null)
        {
            // Pozisyonu ayarla
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
           

            playerTransform.position = exitPoint.position;

            // Yönü ayarla
            if (flipDirection)
            {
                playerTransform.rotation = exitPoint.rotation;
            }

            if (cc != null) cc.enabled = true;

            // Rigidbody varsa velocity sıfırla
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }

        // Kısa bekleme (karanlıkta)
        yield return new WaitForSeconds(0.2f);

        // 4. Kapı açılma sesi (hedef asansörde)
        if (targetElevator != null)
        {
            targetElevator.PlaySound(targetElevator.doorOpenSound);
        }

        // 5. Fade from black
        fadeComplete = false;
        ScreenFader.Instance.FadeFromBlack(() => fadeComplete = true);

        while (!fadeComplete)
        {
            yield return null;
        }

        isTransitioning = false;
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;
            Debug.Log(interactionText);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerTransform = null;
        }
    }

    // Editor'da görselleştirme
    void OnDrawGizmos()
    {
        // Asansör alanı
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(2f, 2.5f, 2f));

        // Exit point
        if (exitPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(exitPoint.position, 0.3f);
            Gizmos.DrawRay(exitPoint.position, exitPoint.forward);
        }

        // Hedef asansöre çizgi
        if (targetElevator != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetElevator.transform.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(2f, 2.5f, 2f));
    }
}
