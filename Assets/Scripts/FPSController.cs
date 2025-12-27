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
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer = ~0; // Varsayılan: tüm layerlar
    [SerializeField] private bool debugGroundCheck = true; // Debug için

    private Rigidbody rb;
    private float rotationX = 0f;
    private bool isGrounded = false;
    private CapsuleCollider capsuleCollider;
    private int playerLayer;

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
        
        // Sıfır sürtünmeli fizik materyali ekle (duvara yapışmayı önler)
        PhysicsMaterial frictionlessMat = new PhysicsMaterial("Frictionless");
        frictionlessMat.dynamicFriction = 0f;
        frictionlessMat.staticFriction = 0f;
        frictionlessMat.frictionCombine = PhysicsMaterialCombine.Minimum;
        frictionlessMat.bounciness = 0f;
        frictionlessMat.bounceCombine = PhysicsMaterialCombine.Minimum;
        capsuleCollider.material = frictionlessMat;

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
        // Collision-based ground check zaten OnCollisionStay'de yapılıyor
        // Bu fonksiyon sadece debug için
        if (debugGroundCheck && Time.frameCount % 30 == 0) // Her 30 frame'de bir log
        {
            Debug.Log($"Ground Check: {isGrounded}");
        }
    }

    // Collision ile zemin kontrolü - daha güvenilir
    void OnCollisionStay(Collision collision)
    {
        // Altımızda bir şey var mı kontrol et
        foreach (ContactPoint contact in collision.contacts)
        {
            // Contact noktası bizim altımızda mı?
            if (contact.point.y < transform.position.y - 0.1f)
            {
                isGrounded = true;
                if (debugGroundCheck && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"Grounded on: {collision.gameObject.name}");
                }
                return;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Collision bittiğinde, hala zemine değiyor muyuz kontrol et
        isGrounded = false;
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
