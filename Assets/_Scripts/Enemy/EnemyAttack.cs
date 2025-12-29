using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private int damageAmount;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            var healthController = collision.gameObject.GetComponent<HealthController>();
            healthController.TakeDamage(damageAmount);
        }
    }
}
