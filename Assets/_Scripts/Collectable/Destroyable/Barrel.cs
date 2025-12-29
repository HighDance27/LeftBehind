using UnityEngine;

public class Barrel : MonoBehaviour
{
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private int explosionDamage = 50;
    private Animator anim;

    private HealthController health;

    private void Awake()
    {
        health = GetComponent<HealthController>();
        anim = GetComponent<Animator>();
    }

    public void Explode()
    {
        // Damage nearby objects
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D col in hitColliders)
        {
            HealthController targetHealth = col.GetComponent<HealthController>();
            MPHealthController mpTarget = col.GetComponent<MPHealthController>();
            if (targetHealth != null && targetHealth != health) // Don’t damage itself
            {
                targetHealth.TakeDamage(explosionDamage);
            }
            if (mpTarget != null && mpTarget != health)
            {
                mpTarget.TakeDamage(explosionDamage);
            }
        }

        anim.SetTrigger("IsDead");
    }
    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

}
