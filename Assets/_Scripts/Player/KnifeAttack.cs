using UnityEngine;

public class KnifeAttack : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float range = 0.9f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTrigger = "Attack";

    private float cooldownTimer = 0f;
    private bool isHolding;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;
        if (isHolding)
            TryAttack();
    }

    public void StartAttack()
    {
        isHolding = true;
        TryAttack();
    }

    public void StopAttack()
    {
        isHolding = false;
    }

    private void TryAttack()
    {
        if (cooldownTimer > 0f) return;
        cooldownTimer = attackCooldown;

        if (animator != null) animator.SetTrigger(attackTrigger);
    }

    public void DealDamage()
    {
        var hits = Physics2D.OverlapCircleAll(attackPoint.position, range, enemyLayers);
        foreach (var hit in hits)
        {
            var health = hit.GetComponent<HealthController>()
                         ?? hit.GetComponentInParent<HealthController>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, range);
    }
}
