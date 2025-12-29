using UnityEngine;

public class MPEnemyCollide : MonoBehaviour
{
    [SerializeField]
    private int damageAmount = 10;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            var mpHealth = collision.gameObject.GetComponent<MPHealthController>();
            mpHealth.TakeDamage(damageAmount);
        }
    }
}
