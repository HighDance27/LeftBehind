using UnityEngine;

public class HealthCollectable : MonoBehaviour, ICollectableBehaviour
{
    [SerializeField] private int healthAmount;

    public void OnCollected(GameObject player)
    {
        player.GetComponent<HealthController>().AddHealth(healthAmount);
    }
}
