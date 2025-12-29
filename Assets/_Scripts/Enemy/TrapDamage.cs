using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    [SerializeField] private int damage;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //var
            HealthController health = other.GetComponent<HealthController>();
            health.TakeDamage(10);
        }
    }
}
