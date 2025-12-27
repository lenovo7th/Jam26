using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 1.5f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Components")]
    [SerializeField] private Rigidbody rb;

    private bool canMove = true;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        // Oyuncuyu otomatik bul
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    void FixedUpdate()
    {
        if (!canMove || target == null) return;

        MoveTowardsTarget();
        RotateTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget > stoppingDistance)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0; // Y ekseninde hareket etme
            
            Vector3 velocity = direction * moveSpeed;
            velocity.y = rb.linearVelocity.y; // Yerçekimini koru
            
            rb.linearVelocity = velocity;
        }
        else
        {
            // Durma mesafesinde, hareketi durdur
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!canMove && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    public float GetDistanceToTarget()
    {
        if (target == null) return float.MaxValue;
        return Vector3.Distance(transform.position, target.position);
    }
}
