using UnityEngine;

/// <summary>
/// Tek dosyada FPS kontrolü.
/// KULLANIM:
/// 1. Player objesine (Capsule) bu scripti ekle
/// 2. Player'a "Player" tag'i ver
/// 3. Main Camera'yı Player'ın içine sürükle (child yap)
/// 4. Play'e bas
/// </summary>
public class FPSController : MonoBehaviour
{
    [Header("Hareket")]
    public float walkSpeed = 6f;
    public float jumpForce = 7f;

    [Header("Mouse")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("Otomatik Ayarlanır")]
    public Transform cameraTransform;

    [Header("Zemin Algılama")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer = ~0; // Varsayılan: tüm layerlar

    private Rigidbody rb;
    private float rotationX = 0f;
    private bool isGrounded = false;
    private CapsuleCollider capsuleCollider;

    void Start()
    {
        // Rigidbody ayarla
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // CapsuleCollider'ı al
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider == null)
        {
            capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            capsuleCollider.height = 2f;
            capsuleCollider.radius = 0.5f;
        }

        // Kamerayı bul
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
                cameraTransform.localPosition = new Vector3(0, 0.5f, 0);
                cameraTransform.localRotation = Quaternion.identity;
            }
            else
            {
                // Kamera yoksa oluştur
                GameObject camObj = new GameObject("PlayerCamera");
                camObj.transform.SetParent(transform);
                camObj.transform.localPosition = new Vector3(0, 0.5f, 0);
                camObj.transform.localRotation = Quaternion.identity;
                Camera newCam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
                cameraTransform = camObj.transform;
            }
        }

        // Cursor'ı kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("FPS Controller hazır! WASD=Hareket, Mouse=Bakış, Space=Zıpla");
    }

    void Update()
    {
        MouseLook();
        CheckGround();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // ESC ile cursor'ı aç
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void MouseLook()
    {
        if (cameraTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Sağ-sol dönüş (player'ı döndür)
        transform.Rotate(0, mouseX, 0);

        // Yukarı-aşağı bakış (kamerayı döndür)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 direction = (transform.forward * v + transform.right * h).normalized;
        Vector3 velocity = direction * walkSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void CheckGround()
    {
        // CapsuleCollider'ın tam altından raycast at
        float colliderHeight = capsuleCollider != null ? capsuleCollider.height : 2f;
        float colliderCenterY = capsuleCollider != null ? capsuleCollider.center.y : 0f;
        
        // Collider'ın alt noktasını hesapla
        float bottomY = colliderCenterY - (colliderHeight / 2f);
        Vector3 rayOrigin = transform.position + new Vector3(0, bottomY + 0.05f, 0);
        
        float rayLength = 0.2f; // Zemine olan mesafe toleransı
        
        // Ana raycast - merkezden
        RaycastHit hit;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, groundLayer, QueryTriggerInteraction.Ignore);
        
        // Debug - Console'da göster
        // Debug.Log($"Ground Check: {isGrounded}, RayOrigin Y: {rayOrigin.y}, Hit: {(hit.collider != null ? hit.collider.name : "none")}");
        
        // Eğer merkez raycast başarısız olduysa, kenarlardankontrol et
        if (!isGrounded)
        {
            float offset = capsuleCollider != null ? capsuleCollider.radius * 0.8f : 0.3f;
            
            // 4 yönden kontrol
            Vector3[] offsets = new Vector3[]
            {
                transform.right * offset,
                -transform.right * offset,
                transform.forward * offset,
                -transform.forward * offset
            };
            
            foreach (Vector3 off in offsets)
            {
                if (Physics.Raycast(rayOrigin + off, Vector3.down, rayLength, groundLayer, QueryTriggerInteraction.Ignore))
                {
                    isGrounded = true;
                    break;
                }
            }
        }
    }

    // Debug için - zemin algılama görselleştirmesi (Scene view'da görünür)
    void OnDrawGizmosSelected()
    {
        if (capsuleCollider != null)
        {
            float radius = capsuleCollider.radius * 0.9f;
            float height = capsuleCollider.height;
            Vector3 spherePosition = transform.position + Vector3.down * (height / 2f - radius);
            
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(spherePosition, radius + groundCheckDistance);
        }
    }
}
