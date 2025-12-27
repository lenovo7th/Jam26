using UnityEngine;
using System.Collections;

/// <summary>
/// BASİT KAPI SİSTEMİ
/// 
/// KURULUM:
/// 1. Kapıya bu scripti ekle
/// 2. Boş bir obje oluştur, kapının menteşe köşesine koy
/// 3. O objeyi "Hinge Point" alanına sürükle
/// 4. Kapıya Box Collider ekle, Is Trigger = true
/// </summary>
public class SimpleDoor : MonoBehaviour
{
    [Header("Menteşe Noktası")]
    [Tooltip("Boş bir obje oluştur, menteşe yerine koy, buraya sürükle")]
    public Transform hingePoint;
    
    [Header("Ayarlar")]
    public float openAngle = 90f;           // Açılma açısı
    public float speed = 2f;                // Açılma hızı
    public bool autoClose = true;           // Oyuncu uzaklaşınca kapat
    
    [Header("Etkileşim Mesafesi")]
    [Tooltip("Oyuncunun kapıya ne kadar yakın olması gerekiyor")]
    public float interactionDistance = 2f;  // Etkileşim mesafesi
    
    [Header("Sesler (Opsiyonel)")]
    public AudioClip openSound;
    public AudioClip closeSound;

    private bool isOpen = false;
    private bool isMoving = false;
    private bool playerNearby = false;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float currentAngle = 0f;
    private AudioSource audioSource;
    private BoxCollider triggerCollider;

    void Start()
    {
        // Başlangıç pozisyon ve rotasyonunu kaydet
        startPosition = transform.position;
        startRotation = transform.rotation;

        // Ses için
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;

        // Collider ayarla
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
        }
        // Collider boyutunu interaction distance'a göre ayarla
        triggerCollider.size = new Vector3(interactionDistance, 2.5f, interactionDistance);
        
        // Hinge point uyarısı
        if (hingePoint == null)
        {
            Debug.LogWarning("[SimpleDoor] Hinge Point atanmamış! Boş obje oluştur ve ata.");
        }
    }

    void Update()
    {
        // Her frame mesafe kontrolü yap
        CheckPlayerDistance();
        
        // E tuşu ile aç/kapat
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !isMoving)
        {
            if (isOpen)
                StartCoroutine(Close());
            else
                StartCoroutine(Open());
        }
    }

    IEnumerator Open()
    {
        if (hingePoint == null)
        {
            Debug.LogError("[SimpleDoor] Hinge Point atanmamış!");
            yield break;
        }
        
        isMoving = true;
        PlaySound(openSound);

        float targetAngle = openAngle;
        
        while (Mathf.Abs(currentAngle - targetAngle) > 0.5f)
        {
            float step = speed * Time.deltaTime * 60f;
            float direction = Mathf.Sign(targetAngle - currentAngle);
            float moveAngle = Mathf.Min(step, Mathf.Abs(targetAngle - currentAngle)) * direction;
            
            transform.RotateAround(hingePoint.position, Vector3.up, moveAngle);
            currentAngle += moveAngle;
            
            yield return null;
        }

        isOpen = true;
        isMoving = false;
    }

    IEnumerator Close()
    {
        if (hingePoint == null)
        {
            Debug.LogError("[SimpleDoor] Hinge Point atanmamış!");
            yield break;
        }
        
        isMoving = true;
        PlaySound(closeSound);

        float targetAngle = 0f;
        
        while (Mathf.Abs(currentAngle - targetAngle) > 0.5f)
        {
            float step = speed * Time.deltaTime * 60f;
            float direction = Mathf.Sign(targetAngle - currentAngle);
            float moveAngle = Mathf.Min(step, Mathf.Abs(targetAngle - currentAngle)) * direction;
            
            transform.RotateAround(hingePoint.position, Vector3.up, moveAngle);
            currentAngle += moveAngle;
            
            yield return null;
        }
        
        // Tam kapanma garantisi
        transform.position = startPosition;
        transform.rotation = startRotation;
        currentAngle = 0f;

        isOpen = false;
        isMoving = false;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    void CheckPlayerDistance()
    {
        // Kamerayı bul (FPS için daha doğru)
        Camera cam = Camera.main;
        if (cam == null) 
        {
            Debug.LogWarning("[SimpleDoor] Main Camera bulunamadı!");
            return;
        }
        
        // Mesafeyi KAMERADAN hesapla
        float distance = Vector3.Distance(transform.position, cam.transform.position);
        
        // Debug - her 60 frame'de bir göster
        if (Time.frameCount % 60 == 0)
        {
            // Debug.Log($"[SimpleDoor] Mesafe: {distance:F1}m, Gerekli: {interactionDistance}m, Yakın mı: {distance <= interactionDistance}");
        }
        
        bool wasNearby = playerNearby;
        playerNearby = distance <= interactionDistance;
        
        // Yeni girdi
        if (playerNearby && !wasNearby)
        {
            Debug.Log("Kapıyı açmak için [E] tuşuna bas");
        }
        
        // Uzaklaştı - otomatik kapat
        if (!playerNearby && wasNearby)
        {
            if (autoClose && isOpen && !isMoving)
                StartCoroutine(Close());
        }
    }

    // Trigger'lar kaldırıldı - mesafe kontrolü Update'de yapılıyor

    // Scene'de menteşe noktasını göster
    void OnDrawGizmos()
    {
        // Menteşe noktası - TURUNCU KÜP
        if (hingePoint != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f); // Turuncu
            Gizmos.DrawCube(hingePoint.position, new Vector3(0.15f, 0.15f, 0.15f));
            Gizmos.DrawWireCube(hingePoint.position, new Vector3(0.15f, 0.15f, 0.15f));
            
            // Menteşe ekseni (dikey çizgi)
            Gizmos.color = Color.red;
            Gizmos.DrawLine(hingePoint.position - Vector3.up * 0.5f, hingePoint.position + Vector3.up * 2.5f);
        }
        else
        {
            // Hinge point yoksa uyarı
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (hingePoint != null)
        {
            // Açılma yönünü göster
            Gizmos.color = Color.yellow;
            Vector3 direction = Quaternion.Euler(0, openAngle / 2, 0) * (transform.position - hingePoint.position).normalized;
            Gizmos.DrawRay(hingePoint.position, direction * 2f);
        }
    }
}
