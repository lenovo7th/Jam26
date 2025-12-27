using UnityEngine;
using System.Collections;

/// <summary>
/// Kapı açılıp kapanma sistemi.
/// KULLANIM:
/// 1. Kapı objesine bu scripti ekle
/// 2. Pivot noktasını kapının menteşe tarafına ayarla (Blender'da veya Empty parent ile)
/// 3. Box Collider ekle ve Is Trigger = true yap (etkileşim alanı için)
/// 4. (Opsiyonel) Ses dosyalarını ata
/// </summary>
public class Door : MonoBehaviour
{
    public enum DoorType
    {
        Rotating,   // Menteşeli kapı (dönerek açılır)
        Sliding     // Sürgülü kapı (kayarak açılır)
    }

    public enum TriggerMode
    {
        Manual,     // E tuşu ile açılır
        Automatic,  // Yaklaşınca otomatik açılır
        Both        // Her ikisi de
    }

    [Header("Kapı Tipi")]
    public DoorType doorType = DoorType.Rotating;
    public TriggerMode triggerMode = TriggerMode.Manual;

    [Header("Dönen Kapı Ayarları")]
    public float rotationAngle = 90f;
    public Vector3 rotationAxis = Vector3.up; // Y ekseni etrafında döner
    [Tooltip("Menteşe noktası offset - kapının köşesine göre ayarla")]
    public Vector3 pivotOffset = Vector3.zero; // Örn: (-0.5, 0, 0) sol kenara taşır

    [Header("Sürgülü Kapı Ayarları")]
    public Vector3 slideDirection = Vector3.right;
    public float slideDistance = 2f;

    [Header("Animasyon")]
    public float openSpeed = 2f;
    public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Sesler")]
    public AudioClip openSound;
    public AudioClip closeSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    [Header("UI İpucu")]
    public string interactionText = "Kapıyı açmak için [E] tuşuna bas";

    [Header("Durum")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private bool isAnimating = false;

    private bool playerInRange = false;
    private Vector3 closedPosition;
    private Quaternion closedRotation;
    private Vector3 openPosition;
    private Quaternion openRotation;
    private AudioSource audioSource;

    void Start()
    {
        // Başlangıç pozisyon ve rotasyonunu kaydet
        closedPosition = transform.localPosition;
        closedRotation = transform.localRotation;

        // Açık pozisyon/rotasyonu hesapla
        if (doorType == DoorType.Rotating)
        {
            openRotation = closedRotation * Quaternion.AngleAxis(rotationAngle, rotationAxis);
            openPosition = closedPosition; // Dönen kapıda pozisyon değişmez
        }
        else // Sliding
        {
            openPosition = closedPosition + slideDirection.normalized * slideDistance;
            openRotation = closedRotation; // Sürgülü kapıda rotasyon değişmez
        }

        // AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D ses

        // Collider kontrolü
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(2f, 2.5f, 2f);
            Debug.Log("[Door] Trigger collider eklendi.");
        }
    }

    void Update()
    {
        // Manuel mod veya Both modunda E tuşu ile aç/kapa
        if (playerInRange && !isAnimating && Input.GetKeyDown(KeyCode.E))
        {
            if (triggerMode == TriggerMode.Manual || triggerMode == TriggerMode.Both)
            {
                ToggleDoor();
            }
        }
    }

    public void ToggleDoor()
    {
        if (isAnimating) return;

        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    public void OpenDoor()
    {
        if (isOpen || isAnimating) return;
        StartCoroutine(AnimateDoor(true));
    }

    public void CloseDoor()
    {
        if (!isOpen || isAnimating) return;
        StartCoroutine(AnimateDoor(false));
    }

    private IEnumerator AnimateDoor(bool opening)
    {
        isAnimating = true;

        // Ses çal
        PlaySound(opening ? openSound : closeSound);

        float elapsed = 0f;
        float duration = 1f / openSpeed;
        
        // Başlangıç değerleri
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        
        if (doorType == DoorType.Rotating && pivotOffset != Vector3.zero)
        {
            // Pivot offset ile döndürme
            float startAngle = opening ? 0f : rotationAngle;
            float endAngle = opening ? rotationAngle : 0f;
            
            // World space'de pivot noktası
            Vector3 pivotWorld = transform.TransformPoint(pivotOffset);
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = openCurve.Evaluate(elapsed / duration);
                float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
                
                // Başlangıç pozisyonuna resetle ve pivot etrafında döndür
                transform.localPosition = closedPosition;
                transform.localRotation = closedRotation;
                
                // Pivot noktası etrafında döndür
                pivotWorld = transform.TransformPoint(pivotOffset);
                transform.RotateAround(pivotWorld, transform.TransformDirection(rotationAxis), currentAngle);
                
                yield return null;
            }
            
            // Son pozisyon
            transform.localPosition = closedPosition;
            transform.localRotation = closedRotation;
            pivotWorld = transform.TransformPoint(pivotOffset);
            transform.RotateAround(pivotWorld, transform.TransformDirection(rotationAxis), endAngle);
        }
        else
        {
            // Normal animasyon (pivot offset yok veya sürgülü kapı)
            Vector3 endPos = opening ? openPosition : closedPosition;
            Quaternion endRot = opening ? openRotation : closedRotation;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = openCurve.Evaluate(elapsed / duration);

                transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                transform.localRotation = Quaternion.Slerp(startRot, endRot, t);

                yield return null;
            }

            // Son pozisyonu garantile
            transform.localPosition = endPos;
            transform.localRotation = endRot;
        }

        isOpen = opening;
        isAnimating = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // Otomatik mod
        if (triggerMode == TriggerMode.Automatic || triggerMode == TriggerMode.Both)
        {
            if (!isOpen)
                OpenDoor();
        }
        else
        {
            Debug.Log(interactionText);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        // Otomatik modda uzaklaşınca kapat
        if (triggerMode == TriggerMode.Automatic || triggerMode == TriggerMode.Both)
        {
            if (isOpen)
                CloseDoor();
        }
    }

    // Editor'da görselleştirme
    void OnDrawGizmos()
    {
        // Kapı alanı
        Gizmos.color = isOpen ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, new Vector3(1f, 2f, 0.1f));

        // Menteşe noktası (Pivot Offset) - TURUNCU KÜP
        if (doorType == DoorType.Rotating && pivotOffset != Vector3.zero)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // Turuncu
            Gizmos.DrawCube(pivotOffset, new Vector3(0.15f, 0.15f, 0.15f));
            Gizmos.DrawWireCube(pivotOffset, new Vector3(0.15f, 0.15f, 0.15f));
        }

        // Açılma yönü
        Gizmos.color = Color.yellow;
        if (doorType == DoorType.Rotating)
        {
            // Dönen kapı için yay çiz
            Vector3 pivotPoint = pivotOffset != Vector3.zero ? pivotOffset : Vector3.zero;
            Vector3 direction = Quaternion.AngleAxis(rotationAngle / 2, rotationAxis) * Vector3.forward;
            Gizmos.DrawRay(pivotPoint, direction * 1.5f);
        }
        else
        {
            // Sürgülü kapı için ok çiz
            Gizmos.DrawRay(Vector3.zero, slideDirection.normalized * slideDistance);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        
        // Kapı çerçevesi
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(1f, 2f, 0.1f));
        
        // Menteşe noktası - daha büyük göster seçildiğinde
        if (doorType == DoorType.Rotating && pivotOffset != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(pivotOffset, new Vector3(0.2f, 2f, 0.2f)); // Tam boy menteşe çizgisi
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // Turuncu
            Gizmos.DrawCube(pivotOffset, new Vector3(0.2f, 0.2f, 0.2f));
        }
    }

    // Public erişim
    public bool IsOpen => isOpen;
    public bool IsAnimating => isAnimating;
}
