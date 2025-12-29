using Photon.Pun;
using UnityEngine;

public class MPKnifeAttack : MonoBehaviourPun
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
        if (animator == null) animator = GetComponent<Animator>();
        if (attackPoint == null) attackPoint = transform; // fallback
    }

    private void Update()
    {
        if (photonView != null && !photonView.IsMine) return;

        cooldownTimer -= Time.deltaTime;
        if (isHolding) TryAttack();
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
        else
            DealDamage(); // không có event trong animator thì tự gọi
    }

    public void DealDamage()
    {
        if (photonView != null && !photonView.IsMine) return;
        var hits = Physics2D.OverlapCircleAll(attackPoint.position, range, enemyLayers);
        foreach (var hit in hits)
        {
            var health = hit.GetComponent<MPHealthController>()
                         ?? hit.GetComponentInParent<MPHealthController>();

            if (health != null)
            {
                health.TakeDamage(damage);

            }
        }
    }

    public void PlayDeathAnimation()
    {
        enabled = false;
        animator.SetTrigger("IsDead");
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, range);
    }

}
