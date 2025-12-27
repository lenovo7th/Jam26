using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRange = 1.5f;

    private float lastAttackTime;
    private Transform target;

    void Awake()
    {
        // Oyuncuyu otomatik bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
    }

    void Update()
    {
        TryAttack();
    }

    private void TryAttack()
    {
        if (target == null) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        if (distanceToTarget <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    private void Attack()
    {
        lastAttackTime = Time.time;
        
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"{gameObject.name} attacked player for {damage} damage!");
        }
    }

    // Alternatif: Collision ile hasar
    void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            lastAttackTime = Time.time;
            playerHealth.TakeDamage(damage);
            Debug.Log($"{gameObject.name} collision attack for {damage} damage!");
        }
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    public void SetAttackCooldown(float newCooldown)
    {
        attackCooldown = newCooldown;
    }
}
